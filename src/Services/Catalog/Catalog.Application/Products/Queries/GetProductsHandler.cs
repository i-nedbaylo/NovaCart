using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Application.Products.Dtos;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

public sealed class GetProductsHandler : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var dtos = items.Select(product => new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Slug,
            product.ImageUrl,
            product.Price.Amount,
            product.Price.Currency,
            product.Status.ToString(),
            product.CategoryId,
            product.Category?.Name,
            product.CreatedAt,
            product.UpdatedAt)).ToList();

        var pagedResult = new PagedResult<ProductDto>(dtos, totalCount, request.PageNumber, request.PageSize);

        return pagedResult;
    }
}
