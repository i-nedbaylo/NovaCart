using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Domain.Entities;

namespace NovaCart.Services.Ordering.Application.Mapping;

internal static class OrderDtoMapper
{
    internal static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id,
            order.BuyerId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.Country,
                order.ShippingAddress.ZipCode),
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)).ToList(),
            order.CreatedAt,
            order.UpdatedAt);
    }
}
