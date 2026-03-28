using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.Services.Payment.Contracts.IntegrationEvents;

public sealed record PaymentSucceededIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
}
