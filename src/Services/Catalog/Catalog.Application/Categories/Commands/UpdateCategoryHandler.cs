using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Categories.Commands;

public sealed class UpdateCategoryHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category", request.Id));

        var existingBySlug = await _categoryRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != request.Id)
            return Result.Failure(Error.Conflict("Category.SlugExists", $"A category with slug '{request.Slug}' already exists."));

        if (request.ParentCategoryId.HasValue)
        {
            if (request.ParentCategoryId.Value == request.Id)
                return Result.Failure(Error.Validation("Category.SelfReference", "A category cannot be its own parent."));

            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is null)
                return Result.Failure(Error.NotFound("Category", request.ParentCategoryId.Value));
        }

        category.Update(request.Name, request.Description, request.Slug, request.ParentCategoryId);

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
