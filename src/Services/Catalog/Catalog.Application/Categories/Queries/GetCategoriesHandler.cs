using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Categories.Dtos;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Services.Catalog.Application.Categories.Queries;

public sealed class GetCategoriesHandler : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        var dtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Slug,
            c.ParentCategoryId,
            c.CreatedAt,
            c.UpdatedAt)).ToList();

        return dtos;
    }
}
