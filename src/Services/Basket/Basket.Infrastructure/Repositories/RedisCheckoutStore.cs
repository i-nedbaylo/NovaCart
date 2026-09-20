using System.Text.Json;
using NovaCart.Services.Basket.Application.Abstractions;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
using StackExchange.Redis;
namespace NovaCart.Services.Basket.Infrastructure.Repositories;

public sealed class RedisCheckoutStore(IConnectionMultiplexer redis) : ICheckoutStore
{
    internal const string PendingKey = "checkout:pending";
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Receipts have no TTL: even a delayed retry must not create a new checkout.
    // Redis AOF persistence is required. This atomic script targets a single Redis instance.
    public async Task<bool> IsAcceptedAsync(string buyerId, Guid revision, CancellationToken ct) =>
        await redis.GetDatabase().KeyExistsAsync(ReceiptKey(buyerId, revision)).WaitAsync(ct);

    public async Task<bool> TryAcceptAsync(ShoppingCart basket, BasketCheckoutIntegrationEvent message, CancellationToken ct)
    {
        const string script = """
            if redis.call('EXISTS', KEYS[2]) == 1 then return 1 end
            local current = redis.call('HGET', KEYS[1], 'data')
            if not current then return 0 end
            local cart = cjson.decode(current)
            if cart.revision ~= ARGV[1] then return 0 end
            redis.call('SET', KEYS[2], ARGV[1])
            redis.call('HSET', KEYS[3], ARGV[1], ARGV[2])
            redis.call('DEL', KEYS[1])
            return 1
            """;
        var result = await redis.GetDatabase().ScriptEvaluateAsync(script,
            [$"basket:{basket.BuyerId}", ReceiptKey(basket.BuyerId, basket.Revision), PendingKey],
            [basket.Revision.ToString(), JsonSerializer.Serialize(message, JsonOptions)]).WaitAsync(ct);
        return (int)result == 1;
    }
    private static string ReceiptKey(string buyerId, Guid revision) => $"checkout:accepted:{buyerId}:{revision}";
}
