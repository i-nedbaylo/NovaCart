namespace NovaCart.Services.Basket.Application.Dtos;

public sealed record UpdateBasketRequest(
    List<UpdateBasketItemRequest> Items);

// Name and price are intentionally omitted: they are resolved server-side from Catalog so a
// client cannot set an arbitrary price (see UpdateBasketHandler).
public sealed record UpdateBasketItemRequest(
    Guid ProductId,
    int Quantity);
