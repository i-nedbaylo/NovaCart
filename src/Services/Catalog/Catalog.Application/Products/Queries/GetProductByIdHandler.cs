using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Products.Dtos;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

public sealed class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Id));

        var dto = new ProductDto(
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
            product.UpdatedAt);

        return dto;
    }
}
