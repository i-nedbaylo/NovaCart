using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Products.Commands;

public sealed class UpdateProductHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("Product", request.Id));

        var existingBySlug = await _productRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != request.Id)
            return Result.Failure(Error.Conflict("Product.SlugExists", $"A product with slug '{request.Slug}' already exists."));

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category", request.CategoryId));

        product.Update(
            request.Name,
            request.Description,
            request.Slug,
            request.PriceAmount,
            request.PriceCurrency,
            request.CategoryId,
            request.ImageUrl);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
