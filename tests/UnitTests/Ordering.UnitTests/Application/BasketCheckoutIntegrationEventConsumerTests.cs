using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Application.Consumers;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class BasketCheckoutIntegrationEventConsumerTests
{
    private readonly IOrderRepository orders = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork uow = Substitute.For<IUnitOfWork>();
    private readonly IOutboxEventCollector events = Substitute.For<IOutboxEventCollector>();
    private BasketCheckoutIntegrationEventConsumer Consumer => new(orders, uow, events, NullLogger<BasketCheckoutIntegrationEventConsumer>.Instance);
    private static BasketCheckoutIntegrationEvent Message => new() {
        BuyerId = Guid.NewGuid().ToString(), Currency = "EUR", PricingVersion = 1,
        Street = "Street", City = "City", State = "State", Country = "Country", ZipCode = "12345",
        Items = [new() { ProductId = Guid.NewGuid(), ProductName = "Accepted product", UnitPrice = 10m, Quantity = 2 }]
    };
    private static ConsumeContext<BasketCheckoutIntegrationEvent> Context(BasketCheckoutIntegrationEvent message)
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutIntegrationEvent>>();
        context.Message.Returns(message);
        return context;
    }
    [Fact]
    public async Task AcceptedQuote_IsPreservedEvenIfCatalogChangesLater()
    {
        var message = Message;
        await Consumer.Consume(Context(message));
        orders.Received(1).Add(Arg.Is<Order>(o => o.SourceMessageId == message.Id && o.TotalAmount == 20m && o.Currency == "EUR"));
        events.Received(1).Add(Arg.Is<OrderCreatedIntegrationEvent>(e => e.TotalAmount == 20m && e.Currency == "EUR"));
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Redelivery_DoesNotCreateAnotherOrder()
    {
        var message = Message;
        orders.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>()).Returns(true);
        await Consumer.Consume(Context(message));
        orders.DidNotReceive().Add(Arg.Any<Order>());
        events.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
    }
    [Fact]
    public async Task LegacyEvent_IsNotSilentlyAcknowledgedOrRepriced()
    {
        var action = () => Consumer.Consume(Context(Message with { PricingVersion = 0 }));
        await action.Should().ThrowAsync<InvalidOperationException>();
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
