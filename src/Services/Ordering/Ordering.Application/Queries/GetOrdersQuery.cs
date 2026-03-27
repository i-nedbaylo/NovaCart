using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Dtos;

namespace NovaCart.Services.Ordering.Application.Queries;

public sealed record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? BuyerId = null) : IQuery<PagedResult<OrderDto>>;
