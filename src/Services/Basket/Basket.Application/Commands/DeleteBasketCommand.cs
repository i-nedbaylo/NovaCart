using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed record DeleteBasketCommand(string BuyerId) : ICommand;
