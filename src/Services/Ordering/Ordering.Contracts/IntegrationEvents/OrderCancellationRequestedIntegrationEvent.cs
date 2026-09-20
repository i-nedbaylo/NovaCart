using NovaCart.BuildingBlocks.EventBus;
namespace NovaCart.Services.Ordering.Contracts.IntegrationEvents;

public sealed record OrderCancellationRequestedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
}
