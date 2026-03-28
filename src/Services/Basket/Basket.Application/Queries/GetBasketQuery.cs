using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Dtos;

namespace NovaCart.Services.Basket.Application.Queries;

public sealed record GetBasketQuery(string BuyerId) : IQuery<BasketDto>;
