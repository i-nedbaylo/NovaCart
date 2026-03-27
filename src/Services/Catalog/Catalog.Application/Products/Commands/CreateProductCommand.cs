using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Catalog.Application.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string Slug,
    decimal PriceAmount,
    string PriceCurrency,
    Guid CategoryId,
    string? ImageUrl = null) : ICommand<Guid>;
