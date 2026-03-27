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
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound("Order", request.Id));

        return order.ToDto();
    }
}
