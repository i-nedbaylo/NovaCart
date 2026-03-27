using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Catalog.Application.Products.Dtos;

namespace NovaCart.Services.Catalog.Application.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
