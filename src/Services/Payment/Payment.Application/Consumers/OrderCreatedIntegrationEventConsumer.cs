using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Application.Options;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Services.Payment.Application.Consumers;

public sealed class OrderCreatedIntegrationEventConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxEventCollector _outboxEventCollector;
    private readonly ILogger<OrderCreatedIntegrationEventConsumer> _logger;
    private readonly PaymentSimulationOptions _simulationOptions;

    // NOTE: Simplified for demo purposes. In production, use a real payment gateway
    // (Stripe, PayPal, etc.) instead of random simulation.

    public OrderCreatedIntegrationEventConsumer(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOutboxEventCollector outboxEventCollector,
        ILogger<OrderCreatedIntegrationEventConsumer> logger,
        IOptions<PaymentSimulationOptions> simulationOptions)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _outboxEventCollector = outboxEventCollector;
        _logger = logger;
        _simulationOptions = simulationOptions.Value;
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
        if (_simulationOptions.ProcessingDelay > TimeSpan.Zero)
        {
            await Task.Delay(_simulationOptions.ProcessingDelay, context.CancellationToken);
        }

        // Simulate success/failure based on configured rate
        var isSuccessful = Random.Shared.Next(100) < _simulationOptions.SuccessRatePercent;

        if (isSuccessful)
        {
            payment.MarkAsSucceeded();

            _outboxEventCollector.Add(new PaymentSucceededIntegrationEvent
            {
                OrderId = message.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CorrelationId = message.CorrelationId
            });

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Payment {PaymentId} succeeded for Order {OrderId}", payment.Id, message.OrderId);
        }
        else
        {
            const string reason = "Payment declined by the payment provider (simulated failure).";
            payment.MarkAsFailed(reason);

            _outboxEventCollector.Add(new PaymentFailedIntegrationEvent
            {
                OrderId = message.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Reason = reason,
                CorrelationId = message.CorrelationId
            });

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogWarning("Payment {PaymentId} failed for Order {OrderId}: {Reason}", payment.Id, message.OrderId, reason);
        }
    }
}
