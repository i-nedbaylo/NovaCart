using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Categories.Commands;

public sealed class CreateCategoryHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingCategory is not null)
            return Result.Failure<Guid>(Error.Conflict("Category.SlugExists", $"A category with slug '{request.Slug}' already exists."));

        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is null)
                return Result.Failure<Guid>(Error.NotFound("Category", request.ParentCategoryId.Value));
        }

        var category = Category.Create(
            request.Name,
            request.Description,
            request.Slug,
            request.ParentCategoryId);

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
