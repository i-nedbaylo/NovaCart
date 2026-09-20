using MassTransit;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class PaymentCancelledConsumer(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IConsumer<PaymentCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PaymentCancelledIntegrationEvent> context)
    {
        var order = await orders.GetByIdAsync(context.Message.OrderId, context.CancellationToken)
            ?? throw new InvalidOperationException("Cancellation confirmation arrived before the order.");
        if (order.Status == OrderStatus.Cancelled) return;
        if (order.Status != OrderStatus.CancellationPending)
            throw new InvalidOperationException("Unexpected cancellation confirmation.");
        order.Cancel();
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
