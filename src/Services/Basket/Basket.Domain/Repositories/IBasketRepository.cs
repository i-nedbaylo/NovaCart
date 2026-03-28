using NovaCart.Services.Basket.Domain.Entities;

namespace NovaCart.Services.Basket.Domain.Repositories;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string buyerId, CancellationToken cancellationToken = default);
    Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task DeleteBasketAsync(string buyerId, CancellationToken cancellationToken = default);
}
