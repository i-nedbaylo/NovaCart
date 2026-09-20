using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.Services.Basket.Contracts.IntegrationEvents;

public sealed record BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public string BuyerId { get; init; } = null!;
    public string Currency { get; init; } = "USD";
    public int PricingVersion { get; init; }

    // Shipping address
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string ZipCode { get; init; } = null!;

    public List<BasketCheckoutItem> Items { get; init; } = [];
}

// Trusted server quote validated by Basket, never prices from the public request.
public sealed record BasketCheckoutItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}
