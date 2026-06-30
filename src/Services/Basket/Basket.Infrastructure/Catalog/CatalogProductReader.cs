using System.Net;
using System.Net.Http.Json;
using NovaCart.Services.Basket.Application.Abstractions;

namespace NovaCart.Services.Basket.Infrastructure.Catalog;

/// <summary>
/// HTTP-based <see cref="ICatalogProductReader"/>. Calls the Catalog service directly via Aspire
/// service discovery (not the public gateway). The shared resilience handler from ServiceDefaults
/// supplies retry/circuit-breaker/timeout.
/// </summary>
public sealed class CatalogProductReader(HttpClient httpClient) : ICatalogProductReader
{
    public async Task<IReadOnlyDictionary<Guid, CatalogProduct>> GetActiveProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var lookups = productIds
            .Distinct()
            .Select(id => FetchAsync(id, cancellationToken));

        var products = await Task.WhenAll(lookups);

        return products
            .OfType<CatalogProduct>()
            .ToDictionary(p => p.Id);
    }

    private async Task<CatalogProduct?> FetchAsync(Guid id, CancellationToken cancellationToken)
    {
        CatalogProductResponse? product;
        try
        {
            product = await httpClient.GetFromJsonAsync<CatalogProductResponse>(
                $"/api/v1/products/{id}", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (product is null
            || !string.Equals(product.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new CatalogProduct(product.Id, product.Name, product.PriceAmount, product.PriceCurrency);
    }

    private sealed record CatalogProductResponse(
        Guid Id,
        string Name,
        decimal PriceAmount,
        string PriceCurrency,
        string Status);
}
