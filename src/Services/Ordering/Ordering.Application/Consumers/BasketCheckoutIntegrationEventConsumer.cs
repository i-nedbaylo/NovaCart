using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class BasketCheckoutIntegrationEventConsumer : IConsumer<BasketCheckoutIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BasketCheckoutIntegrationEventConsumer> _logger;

    public BasketCheckoutIntegrationEventConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<BasketCheckoutIntegrationEventConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing basket checkout for Buyer {BuyerId}, TotalPrice: {TotalPrice}",
            message.BuyerId, message.TotalPrice);

        if (!Guid.TryParse(message.BuyerId, out var buyerId))
        {
            _logger.LogError("Invalid BuyerId format: {BuyerId}. Cannot create order.", message.BuyerId);
            return;
        }

        var address = Address.Create(
            message.Street,
            message.City,
            message.State,
            message.Country,
            message.ZipCode);

        var order = Order.Create(buyerId, address);

        foreach (var item in message.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
        }

        // Auto-confirm: user has explicitly checked out
        order.Confirm();

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        // NOTE: Simplified for demo purposes. There is a window between SaveChanges and Publish
        // where a crash would lose the event. In production, the Outbox Pattern (Phase 2.6)
        // ensures atomic persistence and publication.
        await _publishEndpoint.Publish(new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            BuyerId = buyerId,
            TotalAmount = order.TotalAmount,
            Currency = "USD"
        }, context.CancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created and confirmed from basket checkout for Buyer {BuyerId}",
            order.Id, buyerId);
    }
}
