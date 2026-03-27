namespace NovaCart.Services.Catalog.Contracts;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId);
