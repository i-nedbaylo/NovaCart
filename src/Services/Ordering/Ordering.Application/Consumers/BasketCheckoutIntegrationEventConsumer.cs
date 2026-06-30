using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Application.Abstractions;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class BasketCheckoutIntegrationEventConsumer : IConsumer<BasketCheckoutIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxEventCollector _outboxEventCollector;
    private readonly ICatalogProductReader _catalog;
    private readonly ILogger<BasketCheckoutIntegrationEventConsumer> _logger;

    public BasketCheckoutIntegrationEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOutboxEventCollector outboxEventCollector,
        ICatalogProductReader catalog,
        ILogger<BasketCheckoutIntegrationEventConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _outboxEventCollector = outboxEventCollector;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing basket checkout for Buyer {BuyerId}", message.BuyerId);

        if (!Guid.TryParse(message.BuyerId, out var buyerId))
        {
            _logger.LogError("Invalid BuyerId format: {BuyerId}. Cannot create order.", message.BuyerId);
            return;
        }

        // Idempotency: delivery is at-least-once, so this checkout event can be redelivered.
        // Skip if we've already turned it into an order. (The unique index on source_message_id
        // is the backstop for a concurrent-delivery race.)
        if (await _orderRepository.ExistsBySourceMessageIdAsync(message.Id, context.CancellationToken))
        {
            _logger.LogInformation(
                "Basket checkout {MessageId} already processed; skipping duplicate.", message.Id);
            return;
        }

        // Authoritative pricing: re-price every item from Catalog. The event carries only
        // product id + quantity, so a stale or tampered basket cannot influence the charge.
        var products = await _catalog.GetActiveProductsAsync(
            message.Items.Select(i => i.ProductId).ToList(), context.CancellationToken);

        var address = Address.Create(
            message.Street,
            message.City,
            message.State,
            message.Country,
            message.ZipCode);

        var order = Order.Create(buyerId, address, message.Id);

        foreach (var item in message.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                _logger.LogError(
                    "Cannot create order from checkout {MessageId}: product {ProductId} not found or unavailable.",
                    message.Id, item.ProductId);
                return;
            }

            order.AddItem(item.ProductId, product.Name, product.Price, item.Quantity);
        }

        // Auto-confirm: user has explicitly checked out
        order.Confirm();

        _orderRepository.Add(order);

        _outboxEventCollector.Add(new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            BuyerId = buyerId,
            TotalAmount = order.TotalAmount,
            Currency = "USD",
            CorrelationId = message.CorrelationId
        });

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created and confirmed from basket checkout for Buyer {BuyerId}",
            order.Id, buyerId);
    }
}
