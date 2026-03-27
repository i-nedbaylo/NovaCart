namespace NovaCart.Services.Catalog.Contracts;

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
    string? CategoryName);
