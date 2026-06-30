using System.Net.Http.Json;
using NovaCart.Services.Ordering.Application.Abstractions;

namespace NovaCart.Services.Ordering.Infrastructure.Catalog;

/// <summary>
/// HTTP-based <see cref="ICatalogProductReader"/>. Resolves all requested products in a single
/// batch call to Catalog (via Aspire service discovery, not the public gateway). The shared
/// resilience handler from ServiceDefaults supplies retry/circuit-breaker/timeout.
/// </summary>
public sealed class CatalogProductReader(HttpClient httpClient) : ICatalogProductReader
{
    public async Task<IReadOnlyDictionary<Guid, CatalogProduct>> GetActiveProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = productIds.Distinct().ToList();
        if (distinctIds.Count == 0)
            return new Dictionary<Guid, CatalogProduct>();

        var response = await httpClient.PostAsJsonAsync(
            "/api/v1/products/pricing",
            new ProductPricingRequest(distinctIds),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<CatalogProductResponse>>(cancellationToken)
            ?? [];

        return products
            .Where(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                p => p.Id,
                p => new CatalogProduct(p.Id, p.Name, p.PriceAmount, p.PriceCurrency));
    }

    private sealed record ProductPricingRequest(List<Guid> ProductIds);

    private sealed record CatalogProductResponse(
        Guid Id,
        string Name,
        decimal PriceAmount,
        string PriceCurrency,
        string Status);
}
