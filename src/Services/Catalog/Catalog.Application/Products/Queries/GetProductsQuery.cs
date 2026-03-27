using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Application.Products.Dtos;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResult<ProductDto>>;
