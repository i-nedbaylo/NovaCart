namespace NovaCart.BuildingBlocks.EventBus;

/// <summary>
/// Base class for integration events exchanged between microservices.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
