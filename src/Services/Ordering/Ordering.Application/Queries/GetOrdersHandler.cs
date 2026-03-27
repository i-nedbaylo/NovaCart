using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Services.Ordering.Application.Queries;

public sealed class GetOrdersHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.BuyerId,
            cancellationToken);

        var dtos = items.Select(GetOrderByIdHandler.MapToDto).ToList();

        var pagedResult = new PagedResult<OrderDto>(dtos, totalCount, request.PageNumber, request.PageSize);

        return pagedResult;
    }
}
