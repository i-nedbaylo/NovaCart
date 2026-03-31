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

    public int RetryCount { get; private set; }

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
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = Truncate(error);
    }

    public void IncrementRetryCount(string error)
    {
        RetryCount++;
        Error = Truncate(error);
    }

    /// <summary>
    /// Truncates the error string to fit the database column constraint (2048 chars).
    /// </summary>
    private static string Truncate(string? value)
    {
        const int maxLength = 2048;
        if (value is null) return string.Empty;
        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
