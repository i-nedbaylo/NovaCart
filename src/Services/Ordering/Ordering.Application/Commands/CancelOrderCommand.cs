using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;
