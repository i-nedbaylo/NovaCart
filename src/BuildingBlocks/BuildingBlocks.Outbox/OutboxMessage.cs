namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Represents an integration event persisted in the outbox table
/// for reliable at-least-once delivery to the message broker.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public string EventType { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public string? Error { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string eventType, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}
