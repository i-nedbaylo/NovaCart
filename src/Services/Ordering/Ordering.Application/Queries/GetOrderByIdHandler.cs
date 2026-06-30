using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Application.Mapping;
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

        // Treat "not yours" the same as "not found" so order existence isn't leaked across buyers.
        if (order is null || order.BuyerId != request.BuyerId)
            return Result.Failure<OrderDto>(Error.NotFound("Order", request.Id));

        return order.ToDto();
    }
}
