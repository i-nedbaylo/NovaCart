using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Products.Dtos;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

public sealed class GetProductsByIdsHandler : IQueryHandler<GetProductsByIdsQuery, List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByIdsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
            return new List<ProductDto>();

        var products = await _productRepository.GetByIdsAsync(request.Ids, cancellationToken);

        var dtos = products.Select(product => new ProductDto(
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

        return dtos;
    }
}
