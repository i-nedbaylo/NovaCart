using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Application.Mapping;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Application.Queries;

public sealed class GetBasketHandler : IQueryHandler<GetBasketQuery, BasketDto>
{
    private readonly IBasketRepository _basketRepository;

    public GetBasketHandler(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetBasketAsync(request.BuyerId, cancellationToken);

        if (basket is null)
        {
            // Return an empty basket if none exists
            basket = ShoppingCart.Create(request.BuyerId);
        }

        return Result<BasketDto>.Success(BasketMapper.ToDto(basket));
    }
}
