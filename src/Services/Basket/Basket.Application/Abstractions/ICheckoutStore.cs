using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
namespace NovaCart.Services.Basket.Application.Abstractions;

public interface ICheckoutStore
{
    Task<bool> IsAcceptedAsync(string buyerId, Guid revision, CancellationToken ct);
    Task<bool> TryAcceptAsync(ShoppingCart basket, BasketCheckoutIntegrationEvent message, CancellationToken ct);
}
