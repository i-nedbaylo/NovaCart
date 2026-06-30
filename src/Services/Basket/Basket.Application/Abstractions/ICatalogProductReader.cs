namespace NovaCart.Services.Basket.Application.Abstractions;

/// <summary>
/// Reads authoritative product data (name + price) from the Catalog service so basket items are
/// priced server-side. The client only supplies <c>ProductId</c> and <c>Quantity</c>; price and
/// name are never trusted from the client.
/// </summary>
public interface ICatalogProductReader
{
    /// <summary>
    /// Returns the active products among the requested ids, keyed by id. Unknown or non-active
    /// products are omitted, so the caller can reject items it cannot price.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, CatalogProduct>> GetActiveProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}

public sealed record CatalogProduct(Guid Id, string Name, decimal Price, string Currency);
