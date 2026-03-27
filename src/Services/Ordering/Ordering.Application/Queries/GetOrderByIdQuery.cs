using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Ordering.Application.Dtos;

namespace NovaCart.Services.Ordering.Application.Queries;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;
