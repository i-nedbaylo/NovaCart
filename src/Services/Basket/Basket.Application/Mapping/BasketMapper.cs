using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Domain.Entities;

namespace NovaCart.Services.Basket.Application.Mapping;

internal static class BasketMapper
{
    public static BasketDto ToDto(ShoppingCart cart)
    {
        return new BasketDto(
            cart.BuyerId,
            cart.Items.Select(i => new BasketItemDto(
                i.ProductId,
                i.ProductName,
                i.Price,
                i.Quantity)).ToList(),
            cart.TotalPrice);
    }
}
