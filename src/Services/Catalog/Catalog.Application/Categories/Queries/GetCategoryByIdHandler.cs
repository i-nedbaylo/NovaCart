using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Categories.Dtos;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Categories.Queries;

public sealed class GetCategoryByIdHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category", request.Id));

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Slug,
            category.ParentCategoryId,
            category.CreatedAt,
            category.UpdatedAt);

        return dto;
    }
}
