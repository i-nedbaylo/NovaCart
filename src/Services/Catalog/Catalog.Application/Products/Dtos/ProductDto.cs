namespace NovaCart.Services.Catalog.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    string? ImageUrl,
    decimal PriceAmount,
    string PriceCurrency,
    string Status,
    Guid CategoryId,
    string? CategoryName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
