using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Application.Consumers;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class BasketCheckoutIntegrationEventConsumerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxEventCollector _outboxEventCollector = Substitute.For<IOutboxEventCollector>();
    private readonly BasketCheckoutIntegrationEventConsumer _consumer;

    public BasketCheckoutIntegrationEventConsumerTests()
    {
        _consumer = new BasketCheckoutIntegrationEventConsumer(
            _orderRepository,
            _unitOfWork,
            _outboxEventCollector,
            Substitute.For<ILogger<BasketCheckoutIntegrationEventConsumer>>());
    }

    [Fact]
    public async Task Consume_Should_CreateOrderTaggedWithSourceMessageId_When_NotYetProcessed()
    {
        var message = CreateMessage();
        _orderRepository.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        Order? created = null;
        _orderRepository.When(r => r.Add(Arg.Any<Order>()))
            .Do(call => created = call.Arg<Order>());

        await _consumer.Consume(CreateConsumeContext(message));

        _orderRepository.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.Received(1).Add(Arg.Any<IntegrationEvent>());

        created.Should().NotBeNull();
        created!.SourceMessageId.Should().Be(message.Id);
        created.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Consume_Should_SkipDuplicate_When_MessageAlreadyProcessed()
    {
        var message = CreateMessage();
        _orderRepository.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        await _consumer.Consume(CreateConsumeContext(message));

        // Idempotent skip: no order created, nothing saved, no event published.
        _orderRepository.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
    }

    private static BasketCheckoutIntegrationEvent CreateMessage() => new()
    {
        BuyerId = Guid.NewGuid().ToString(),
        TotalPrice = 39.98m,
        Street = "1 Test Street",
        City = "Testville",
        State = "TS",
        Country = "Testland",
        ZipCode = "12345",
        Items =
        [
            new BasketCheckoutItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "E2E Widget",
                Price = 19.99m,
                Quantity = 2
            }
        ]
    };

    private static ConsumeContext<T> CreateConsumeContext<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
