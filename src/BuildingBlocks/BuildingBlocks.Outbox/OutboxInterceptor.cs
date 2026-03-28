using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// EF Core SaveChanges interceptor that captures pending integration events
/// from <see cref="IOutboxEventCollector"/> and persists them as
/// <see cref="OutboxMessage"/> entities atomically within the same transaction.
/// Registered as a scoped service.
/// </summary>
public sealed class OutboxInterceptor(IOutboxEventCollector collector) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var pendingEvents = collector.GetPendingEvents();
        if (pendingEvents.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var @event in pendingEvents)
        {
            var type = @event.GetType();
            var assemblyName = type.Assembly.GetName().Name;
            var eventType = $"{type.FullName}, {assemblyName}";

            var payload = JsonSerializer.Serialize(@event, type, JsonOptions);

            var outboxMessage = OutboxMessage.Create(eventType, payload);
            dbContext.Set<OutboxMessage>().Add(outboxMessage);
        }

        collector.Clear();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
