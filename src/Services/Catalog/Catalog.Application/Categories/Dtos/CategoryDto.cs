namespace NovaCart.Services.Catalog.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
