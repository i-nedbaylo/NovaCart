using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.Services.Basket.Contracts.IntegrationEvents;

public sealed record BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public string BuyerId { get; init; } = null!;
    public decimal TotalPrice { get; init; }

    // Shipping address
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string ZipCode { get; init; } = null!;

    // Items snapshot at checkout time
    public List<BasketCheckoutItem> Items { get; init; } = [];
}

public sealed record BasketCheckoutItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}
