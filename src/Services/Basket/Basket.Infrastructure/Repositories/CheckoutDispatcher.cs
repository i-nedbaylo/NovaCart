using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using StackExchange.Redis;
namespace NovaCart.Services.Basket.Infrastructure.Repositories;

public sealed class CheckoutDispatcher(IConnectionMultiplexer redis, IServiceScopeFactory scopes,
    ILogger<CheckoutDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = redis.GetDatabase();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Crash after Publish/before HDEL is safe: Ordering deduplicates the stable ID.
                await foreach (var entry in db.HashScanAsync(RedisCheckoutStore.PendingKey).WithCancellation(stoppingToken))
                {
                    try
                    {
                        var message = JsonSerializer.Deserialize<BasketCheckoutIntegrationEvent>((string)entry.Value!, RedisCheckoutStore.JsonOptions)
                            ?? throw new InvalidOperationException("Invalid checkout event.");
                        using var scope = scopes.CreateScope();
                        await scope.ServiceProvider.GetRequiredService<IPublishEndpoint>().Publish(message, stoppingToken);
                        await db.HashDeleteAsync(RedisCheckoutStore.PendingKey, entry.Name);
                    }
                    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                    {
                        logger.LogError(ex, "Checkout {CheckoutId} remains pending for retry", entry.Name.ToString());
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Cannot dispatch persisted checkouts; retrying");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
