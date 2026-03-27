namespace NovaCart.BuildingBlocks.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
