using MassTransit;
using Microsoft.Extensions.Logging;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
namespace NovaCart.Services.Ordering.Application.Consumers;

public sealed class BasketCheckoutIntegrationEventConsumer(IOrderRepository orders, IUnitOfWork unitOfWork,
    IOutboxEventCollector events, ILogger<BasketCheckoutIntegrationEventConsumer> logger)
    : IConsumer<BasketCheckoutIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutIntegrationEvent> context)
    {
        var message = context.Message;
        if (await orders.ExistsBySourceMessageIdAsync(message.Id, context.CancellationToken)) return;
        // Legacy/untrusted messages must fail visibly, never be acknowledged without a result.
        if (message.PricingVersion != 1 || !Guid.TryParse(message.BuyerId, out var buyerId) || message.Items.Count == 0)
            throw new InvalidOperationException("Checkout requires a validated server quote (pricing version 1).");
        var address = Address.Create(message.Street, message.City, message.State, message.Country, message.ZipCode);
        var order = Order.Create(buyerId, address, message.Id, message.Currency);
        foreach (var item in message.Items)
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        order.Confirm();
        orders.Add(order);
        events.Add(new OrderCreatedIntegrationEvent {
            OrderId = order.Id, BuyerId = buyerId, TotalAmount = order.TotalAmount,
            Currency = order.Currency, CorrelationId = message.CorrelationId
        });
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Created order {OrderId} from accepted checkout {CheckoutId}", order.Id, message.Id);
    }
}
