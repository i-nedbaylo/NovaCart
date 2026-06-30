using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Products.Dtos;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

/// <summary>
/// Batch lookup used by other services (Basket/Ordering) to resolve authoritative pricing for a
/// set of products in a single call, instead of one request per product.
/// </summary>
public sealed record GetProductsByIdsQuery(IReadOnlyCollection<Guid> Ids) : IQuery<List<ProductDto>>;
