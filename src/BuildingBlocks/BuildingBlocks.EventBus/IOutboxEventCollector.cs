namespace NovaCart.BuildingBlocks.EventBus;

/// <summary>
/// Collects integration events to be persisted atomically with business data
/// via the Outbox Pattern. Events are stored in the outbox table during SaveChanges
/// and published to the message broker by a background processor.
/// </summary>
public interface IOutboxEventCollector
{
    void Add(IntegrationEvent @event);
    IReadOnlyList<IntegrationEvent> GetPendingEvents();
    void Clear();
}
