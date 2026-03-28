using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;

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

        // Idempotency: skip if payment already exists for this order (retry/redelivery)
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (existingPayment is not null)
        {
            _logger.LogWarning(
                "Payment {PaymentId} already exists for Order {OrderId} with status {Status}. Skipping.",
                existingPayment.Id, message.OrderId, existingPayment.Status);
            return;
        }

        var payment = PaymentRecord.Create(message.OrderId, message.TotalAmount, message.Currency);
        _paymentRepository.Add(payment);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

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
