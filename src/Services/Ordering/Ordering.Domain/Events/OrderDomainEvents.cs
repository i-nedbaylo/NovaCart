using NovaCart.BuildingBlocks.Common;

namespace NovaCart.Services.Ordering.Domain.Events;

public sealed record OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent;

public sealed record OrderCancelledDomainEvent(Guid OrderId) : IDomainEvent;

public sealed record OrderStatusChangedDomainEvent(Guid OrderId, string OldStatus, string NewStatus) : IDomainEvent;
