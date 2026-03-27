namespace NovaCart.Services.Ordering.Application.Dtos;

public sealed record OrderDto(
    Guid Id,
    Guid BuyerId,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount,
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
