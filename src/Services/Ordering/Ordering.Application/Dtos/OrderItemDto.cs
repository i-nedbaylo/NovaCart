namespace NovaCart.Services.Ordering.Application.Dtos;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
