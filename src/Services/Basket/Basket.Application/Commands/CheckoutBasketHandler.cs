using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Abstractions;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Repositories;
namespace NovaCart.Services.Basket.Application.Commands;

public sealed class CheckoutBasketHandler(IBasketRepository baskets, ICatalogProductReader catalog, ICheckoutStore checkouts)
    : ICommandHandler<CheckoutBasketCommand>
{
    public async Task<Result> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        if (request.BasketRevision == Guid.Empty)
            return Result.Failure(Error.Validation("Basket.RevisionRequired", "Update your cart before checkout."));
        if (await checkouts.IsAcceptedAsync(request.BuyerId, request.BasketRevision, cancellationToken))
            return Result.Success();
        var basket = await baskets.GetBasketAsync(request.BuyerId, cancellationToken);
        if (basket is null || basket.Revision != request.BasketRevision)
        {
            // Another request may have accepted the revision after our first receipt lookup.
            if (await checkouts.IsAcceptedAsync(request.BuyerId, request.BasketRevision, cancellationToken))
                return Result.Success();
            return basket is null ? Result.Failure(Error.NotFound("Basket", request.BuyerId)) : Changed();
        }
        if (basket.Items.Count == 0)
            return Result.Failure(Error.Validation("Basket.Empty", "Your cart is empty."));

        var products = await catalog.GetActiveProductsAsync(basket.Items.Select(i => i.ProductId).ToArray(), cancellationToken);
        foreach (var item in basket.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                return Result.Failure(Error.Conflict("Basket.UnavailableProduct", "An item is no longer available. Review your cart."));
            if (product.Price != item.Price || product.Currency != basket.Currency) return Changed();
        }
        var message = new BasketCheckoutIntegrationEvent
        {
            Id = basket.Revision, CorrelationId = basket.Revision,
            BuyerId = request.BuyerId, Currency = basket.Currency, PricingVersion = 1,
            Street = request.Street, City = request.City, State = request.State,
            Country = request.Country, ZipCode = request.ZipCode,
            Items = basket.Items.Select(i => new BasketCheckoutItem {
                ProductId = i.ProductId, Quantity = i.Quantity,
                ProductName = products[i.ProductId].Name, UnitPrice = i.Price
            }).ToList()
        };
        return await checkouts.TryAcceptAsync(basket, message, cancellationToken) ? Result.Success() : Changed();
    }
    private static Result Changed() => Result.Failure(Error.Conflict("Basket.Changed",
        "Your cart or its prices have changed. Update the cart and review the new total before checkout."));
}
