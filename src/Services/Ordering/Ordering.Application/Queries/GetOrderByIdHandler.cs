using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Services.Ordering.Application.Queries;

public sealed class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound("Order", request.Id));

        return MapToDto(order);
    }

    internal static OrderDto MapToDto(Order order)
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
