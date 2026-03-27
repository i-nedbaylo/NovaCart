using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Catalog.Application.Categories.Commands;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId = null) : ICommand;
