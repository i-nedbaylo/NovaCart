using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Ordering.Application.Dtos;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed record CreateOrderCommand(
    Guid BuyerId,
    AddressDto ShippingAddress,
    List<CreateOrderItemRequest> Items) : ICommand<Guid>;
