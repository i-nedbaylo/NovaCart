using MassTransit;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Domain.ValueObjects;
namespace NovaCart.Services.Payment.Application.Consumers;

public sealed class OrderCancellationRequestedConsumer(IPaymentRepository payments, IUnitOfWork unitOfWork,
    IOutboxEventCollector events) : IConsumer<OrderCancellationRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCancellationRequestedIntegrationEvent> context)
    {
        var message = context.Message;
        var payment = await payments.GetByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (payment is not null && (payment.Status.Equals(PaymentStatus.Cancelled) || payment.Status.Equals(PaymentStatus.Refunded)))
            return; // Confirmation was committed in the same transaction.
        if (payment is null)
        {
            // Tombstone also prevents a delayed OrderCreated event from initiating payment.
            payment = PaymentRecord.Create(message.OrderId, message.Amount, message.Currency);
            payments.Add(payment);
        }
        // Simulation only, as with the existing payment provider. A real provider must confirm
        // its idempotent void/refund BEFORE this transition is committed.
        payment.CancelOrRefund();
        events.Add(new PaymentCancelledIntegrationEvent {
            OrderId = message.OrderId, PaymentId = payment.Id, CorrelationId = message.CorrelationId
        });
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
