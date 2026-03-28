using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.Services.Ordering.Contracts.IntegrationEvents;

public sealed record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
}
