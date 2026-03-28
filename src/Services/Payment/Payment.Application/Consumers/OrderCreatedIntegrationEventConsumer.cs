using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Services.Payment.Application.Consumers;

public sealed class OrderCreatedIntegrationEventConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderCreatedIntegrationEventConsumer> _logger;

    // NOTE: Simplified for demo purposes. In production, use a real payment gateway
    // (Stripe, PayPal, etc.) instead of random simulation.

    public OrderCreatedIntegrationEventConsumer(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderCreatedIntegrationEventConsumer> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing payment for Order {OrderId}, Amount: {Amount} {Currency}",
            message.OrderId, message.TotalAmount, message.Currency);

        // Idempotency: check if payment already exists for this order (retry/redelivery)
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(message.OrderId, context.CancellationToken);

        PaymentRecord payment;

        if (existingPayment is not null)
        {
            if (!existingPayment.Status.Equals(PaymentStatus.Pending))
            {
                // Already fully processed (Succeeded or Failed) — idempotent skip.
                // NOTE: Simplified for demo purposes. If a crash occurs between SaveChanges
                // (status update) and Publish (integration event), the event will be lost.
                // In production, the Outbox Pattern (Phase 2.6) ensures atomic persistence
                // and publication of events, eliminating this window.
                _logger.LogInformation(
                    "Payment {PaymentId} already processed for Order {OrderId} with status {Status}. Skipping.",
                    existingPayment.Id, message.OrderId, existingPayment.Status);
                return;
            }

            // Existing Pending payment found — previous processing crashed before completing.
            // Resume processing with the existing record.
            _logger.LogWarning(
                "Resuming processing for Pending payment {PaymentId} for Order {OrderId}.",
                existingPayment.Id, message.OrderId);
            payment = existingPayment;
        }
        else
        {
            payment = PaymentRecord.Create(message.OrderId, message.TotalAmount, message.Currency);
            _paymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        // Simulate payment processing delay
        await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);

        // Simulate 80% success rate
        var isSuccessful = Random.Shared.Next(100) < 80;

        if (isSuccessful)
        {
            payment.MarkAsSucceeded();
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Payment {PaymentId} succeeded for Order {OrderId}", payment.Id, message.OrderId);

            await _publishEndpoint.Publish(new PaymentSucceededIntegrationEvent
            {
                OrderId = message.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CorrelationId = message.CorrelationId
            }, context.CancellationToken);
        }
        else
        {
            const string reason = "Payment declined by the payment provider (simulated failure).";
            payment.MarkAsFailed(reason);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogWarning("Payment {PaymentId} failed for Order {OrderId}: {Reason}", payment.Id, message.OrderId, reason);

            await _publishEndpoint.Publish(new PaymentFailedIntegrationEvent
            {
                OrderId = message.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Reason = reason,
                CorrelationId = message.CorrelationId
            }, context.CancellationToken);
        }
    }
}
