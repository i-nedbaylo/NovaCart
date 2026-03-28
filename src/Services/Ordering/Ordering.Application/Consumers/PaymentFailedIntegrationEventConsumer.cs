using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;

namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class PaymentFailedIntegrationEventConsumer : IConsumer<PaymentFailedIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentFailedIntegrationEventConsumer> _logger;

    public PaymentFailedIntegrationEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentFailedIntegrationEventConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing payment failed for Order {OrderId}, Payment {PaymentId}, Reason: {Reason}",
            message.OrderId, message.PaymentId, message.Reason);

        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found. Cannot cancel.", message.OrderId);
            return;
        }

        // Idempotency: skip if already cancelled
        if (order.Status == OrderStatus.Cancelled)
        {
            _logger.LogInformation("Order {OrderId} is already cancelled. Skipping.", message.OrderId);
            return;
        }

        // Do not cancel orders that have already been paid, shipped, or delivered.
        // PaymentFailed arriving late (e.g., duplicate/redelivery) should not revert progress.
        if (order.Status is OrderStatus.Paid or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            _logger.LogWarning(
                "Received payment failed for Order {OrderId} in status {Status}. " +
                "Order has already progressed past payment, skipping cancellation.",
                message.OrderId, order.Status);
            return;
        }

        order.Cancel();

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Order {OrderId} cancelled due to payment failure. Reason: {Reason}",
            message.OrderId, message.Reason);
    }
}
