using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Background service that polls the outbox table for unprocessed messages
/// and publishes them to the message broker via MassTransit.
/// Generic over TDbContext so each service has its own processor instance.
/// </summary>
/// <remarks>
/// Uses PostgreSQL-specific <c>FOR UPDATE SKIP LOCKED</c> to atomically claim
/// messages, preventing duplicate processing when multiple service instances run.
/// </remarks>
public sealed class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private readonly OutboxOptions _options = outboxOptions.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxProcessor<{DbContext}> started", typeof(TDbContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }
        }

        logger.LogInformation("OutboxProcessor<{DbContext}> stopped", typeof(TDbContext).Name);
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // NOTE: PostgreSQL-specific FOR UPDATE SKIP LOCKED ensures atomic
        // message claiming — concurrent instances won't process the same messages.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var messages = await dbContext.Set<OutboxMessage>()
            .FromSql($"""
                SELECT * FROM outbox_messages
                WHERE processed_at IS NULL
                ORDER BY created_at
                LIMIT {_options.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(ct);
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    logger.LogWarning("Cannot resolve type '{EventType}' for outbox message {MessageId}",
                        message.EventType, message.Id);
                    message.MarkAsFailed($"Cannot resolve type: {message.EventType}");
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
                if (@event is null)
                {
                    logger.LogWarning("Failed to deserialize outbox message {MessageId}", message.Id);
                    message.MarkAsFailed("Deserialization returned null");
                    continue;
                }

                await publishEndpoint.Publish(@event, eventType, ct);
                message.MarkAsProcessed();

                logger.LogDebug("Published outbox message {MessageId} of type {EventType}",
                    message.Id, message.EventType);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                message.IncrementRetryCount(ex.Message);

                if (message.RetryCount >= _options.MaxRetries)
                {
                    logger.LogError(ex,
                        "Outbox message {MessageId} exceeded max retries ({MaxRetries}). Marking as failed",
                        message.Id, _options.MaxRetries);
                    message.MarkAsFailed($"Exceeded max retries ({_options.MaxRetries}). Last error: {ex.Message}");
                }
                else
                {
                    logger.LogWarning(ex,
                        "Failed to publish outbox message {MessageId} (attempt {RetryCount}/{MaxRetries}). Will retry",
                        message.Id, message.RetryCount, _options.MaxRetries);
                }
            }
        }

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    }
