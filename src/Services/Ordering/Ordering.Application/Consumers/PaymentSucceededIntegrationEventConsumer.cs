using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;

namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class PaymentSucceededIntegrationEventConsumer : IConsumer<PaymentSucceededIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentSucceededIntegrationEventConsumer> _logger;

    public PaymentSucceededIntegrationEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentSucceededIntegrationEventConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing payment succeeded for Order {OrderId}, Payment {PaymentId}",
            message.OrderId, message.PaymentId);

        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found. Cannot mark as paid.", message.OrderId);
            return;
        }

        // Idempotency: skip if already paid or in post-payment status
        if (order.Status == OrderStatus.Paid)
        {
            _logger.LogInformation("Order {OrderId} is already paid. Skipping.", message.OrderId);
            return;
        }

        // Guard against terminal or post-payment statuses where marking as paid is invalid
        if (order.Status is OrderStatus.Cancelled or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            _logger.LogWarning(
                "Received payment succeeded for Order {OrderId} in status {Status}. Skipping.",
                message.OrderId, order.Status);
            return;
        }

        // Auto-confirm if still in Created status (e.g., API-created orders that triggered payment)
        if (order.Status == OrderStatus.Created)
        {
            order.Confirm();
        }

        order.MarkAsPaid();

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Order {OrderId} marked as paid.", message.OrderId);
    }
}
