namespace NovaCart.Services.Basket.Application.Dtos;

public sealed record BasketDto(
    string BuyerId,
    List<BasketItemDto> Items,
    decimal TotalPrice, Guid Revision = default, string Currency = "USD");

public sealed record BasketItemDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
