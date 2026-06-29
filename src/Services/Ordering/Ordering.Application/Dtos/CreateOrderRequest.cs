namespace NovaCart.Services.Ordering.Application.Dtos;

// BuyerId is intentionally omitted: it is taken from the authenticated user's token
// in the API layer, not from the request body, so a client cannot order on behalf of others.
public sealed record CreateOrderRequest(
    AddressDto ShippingAddress,
    List<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
