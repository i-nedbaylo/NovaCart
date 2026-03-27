using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Products.Commands;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingProduct is not null)
            return Result.Failure<Guid>(Error.Conflict("Product.SlugExists", $"A product with slug '{request.Slug}' already exists."));

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<Guid>(Error.NotFound("Category", request.CategoryId));

        var product = Product.Create(
            request.Name,
            request.Description,
            request.Slug,
            request.PriceAmount,
            request.PriceCurrency,
            request.CategoryId,
            request.ImageUrl);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
