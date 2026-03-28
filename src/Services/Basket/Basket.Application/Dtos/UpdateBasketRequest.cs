namespace NovaCart.Services.Basket.Application.Dtos;

public sealed record UpdateBasketRequest(
    List<UpdateBasketItemRequest> Items);

public sealed record UpdateBasketItemRequest(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
