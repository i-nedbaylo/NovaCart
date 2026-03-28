using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Scoped implementation of <see cref="IOutboxEventCollector"/>.
/// Collects integration events during request/consumer processing.
/// The <see cref="OutboxInterceptor"/> reads these events during SaveChanges
/// and persists them atomically in the outbox table.
/// </summary>
public sealed class OutboxEventCollector : IOutboxEventCollector
{
    private readonly List<IntegrationEvent> _events = [];

    public void Add(IntegrationEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _events.Add(@event);
    }

    public IReadOnlyList<IntegrationEvent> GetPendingEvents() => _events.AsReadOnly();

    public void Clear() => _events.Clear();
}
