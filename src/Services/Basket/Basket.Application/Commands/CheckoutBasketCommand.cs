using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed record CheckoutBasketCommand(
    string BuyerId,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode) : ICommand;
