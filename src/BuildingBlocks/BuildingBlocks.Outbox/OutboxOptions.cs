namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Configuration options for the Outbox Pattern processor.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Number of messages to fetch per polling cycle.
    /// Default: 20.
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Maximum number of publish retries before marking a message as permanently failed.
    /// Default: 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Delay between polling cycles.
    /// Default: 5 seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}
