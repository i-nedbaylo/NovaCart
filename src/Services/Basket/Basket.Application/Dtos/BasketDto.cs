namespace NovaCart.Services.Basket.Application.Dtos;

public sealed record BasketDto(
    string BuyerId,
    List<BasketItemDto> Items,
    decimal TotalPrice);

public sealed record BasketItemDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
