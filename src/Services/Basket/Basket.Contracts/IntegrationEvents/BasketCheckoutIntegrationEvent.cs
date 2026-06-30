using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.Services.Basket.Contracts.IntegrationEvents;

public sealed record BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public string BuyerId { get; init; } = null!;

    // Shipping address
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string ZipCode { get; init; } = null!;

    public List<BasketCheckoutItem> Items { get; init; } = [];
}

// Only the product id and quantity travel in the event. The Ordering service re-prices each
// item from Catalog, so a stale or tampered basket cannot influence the charged amount.
public sealed record BasketCheckoutItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
