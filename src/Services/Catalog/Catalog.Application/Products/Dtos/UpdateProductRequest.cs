namespace NovaCart.Services.Catalog.Application.Products.Dtos;

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    string Slug,
    decimal PriceAmount,
    string PriceCurrency,
    Guid CategoryId,
    string? ImageUrl = null);
