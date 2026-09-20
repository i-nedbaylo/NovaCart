using NovaCart.BuildingBlocks.EventBus;
namespace NovaCart.Services.Payment.Contracts.IntegrationEvents;

public sealed record PaymentCancelledIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
}
