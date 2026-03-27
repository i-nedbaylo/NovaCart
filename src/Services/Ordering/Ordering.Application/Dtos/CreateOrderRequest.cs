namespace NovaCart.Services.Ordering.Application.Dtos;

public sealed record CreateOrderRequest(
    Guid BuyerId,
    AddressDto ShippingAddress,
    List<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
