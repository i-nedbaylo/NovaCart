namespace NovaCart.Services.Ordering.Contracts;

public sealed record OrderDto(
    Guid Id,
    Guid BuyerId,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items);
