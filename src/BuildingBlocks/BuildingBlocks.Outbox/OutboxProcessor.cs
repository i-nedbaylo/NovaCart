using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Background service that polls the outbox table for unprocessed messages
/// and publishes them to the message broker via MassTransit.
/// Generic over TDbContext so each service has its own processor instance.
/// </summary>
/// <remarks>
/// NOTE: Simplified for demo purposes. In production, consider using
/// a distributed lock (e.g., PostgreSQL advisory locks) to prevent
/// concurrent processing in scaled-out scenarios.
/// </remarks>
public sealed class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

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
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        logger.LogInformation("OutboxProcessor<{DbContext}> stopped", typeof(TDbContext).Name);
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = ResolveType(message.EventType);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                message.MarkAsFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static Type? ResolveType(string fullName)
    {
        var type = Type.GetType(fullName);
        if (type is not null)
            return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(fullName);
            if (type is not null)
                return type;
        }

        return null;
    }
}
