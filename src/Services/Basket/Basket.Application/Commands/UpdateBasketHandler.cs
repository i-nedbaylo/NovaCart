using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Application.Mapping;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class UpdateBasketHandler : ICommandHandler<UpdateBasketCommand, BasketDto>
{
    private readonly IBasketRepository _basketRepository;

    public UpdateBasketHandler(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task<Result<BasketDto>> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = ShoppingCart.Create(request.BuyerId);

        foreach (var item in request.Items)
        {
            basket.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
        }

        var updated = await _basketRepository.UpdateBasketAsync(basket, cancellationToken);

        return Result<BasketDto>.Success(BasketMapper.ToDto(updated));
    }
}
