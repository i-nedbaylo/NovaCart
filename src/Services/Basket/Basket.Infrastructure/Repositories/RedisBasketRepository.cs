using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Infrastructure.Repositories;

public sealed class RedisBasketRepository : IBasketRepository
{
    private readonly IDistributedCache _cache;

    // NOTE: Simplified for demo purposes. In production, TTL should be configurable via IOptions.
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RedisBasketRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<ShoppingCart?> GetBasketAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(GetKey(buyerId), cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<ShoppingCart>(json, JsonOptions);
    }

    public async Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(basket, JsonOptions);

        await _cache.SetStringAsync(GetKey(basket.BuyerId), json, CacheOptions, cancellationToken);

        return basket;
    }

    public async Task DeleteBasketAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(GetKey(buyerId), cancellationToken);
    }

    private static string GetKey(string buyerId) => $"basket:{buyerId}";
}
