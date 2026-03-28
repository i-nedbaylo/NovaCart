using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Dtos;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed record UpdateBasketCommand(
    string BuyerId,
    List<UpdateBasketItemRequest> Items) : ICommand<BasketDto>;
