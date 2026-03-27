using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Categories.Dtos;

namespace NovaCart.Services.Catalog.Application.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryDto>;
