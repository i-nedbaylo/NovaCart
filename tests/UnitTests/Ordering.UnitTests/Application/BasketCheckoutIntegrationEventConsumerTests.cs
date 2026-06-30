using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Application.Abstractions;
using NovaCart.Services.Ordering.Application.Consumers;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class BasketCheckoutIntegrationEventConsumerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxEventCollector _outboxEventCollector = Substitute.For<IOutboxEventCollector>();
    private readonly ICatalogProductReader _catalog = Substitute.For<ICatalogProductReader>();
    private readonly BasketCheckoutIntegrationEventConsumer _consumer;

    public BasketCheckoutIntegrationEventConsumerTests()
    {
        _consumer = new BasketCheckoutIntegrationEventConsumer(
            _orderRepository,
            _unitOfWork,
            _outboxEventCollector,
            _catalog,
            Substitute.For<ILogger<BasketCheckoutIntegrationEventConsumer>>());
    }

    private void SetupCatalog(params CatalogProduct[] products)
    {
        IReadOnlyDictionary<Guid, CatalogProduct> map = products.ToDictionary(p => p.Id);
        _catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(map);
    }

    [Fact]
    public async Task Consume_Should_CreateOrderPricedFromCatalog_When_NotYetProcessed()
    {
        var product = new CatalogProduct(Guid.NewGuid(), "Headphones", 79.99m, "USD");
        SetupCatalog(product);

        var message = CreateMessage(product.Id, quantity: 2);
        _orderRepository.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        Order? created = null;
        _orderRepository.When(r => r.Add(Arg.Any<Order>())).Do(call => created = call.Arg<Order>());

        await _consumer.Consume(CreateConsumeContext(message));

        _orderRepository.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.Received(1).Add(Arg.Any<IntegrationEvent>());

        created.Should().NotBeNull();
        created!.SourceMessageId.Should().Be(message.Id);
        created.Items.Should().ContainSingle();
        created.TotalAmount.Should().Be(79.99m * 2); // priced from Catalog, not the event
    }

    [Fact]
    public async Task Consume_Should_SkipDuplicate_When_MessageAlreadyProcessed()
    {
        var message = CreateMessage(Guid.NewGuid(), quantity: 1);
        _orderRepository.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        await _consumer.Consume(CreateConsumeContext(message));

        _orderRepository.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task Consume_Should_NotCreateOrder_When_ProductNotInCatalog()
    {
        SetupCatalog(); // product unknown or not active
        var message = CreateMessage(Guid.NewGuid(), quantity: 1);
        _orderRepository.ExistsBySourceMessageIdAsync(message.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        await _consumer.Consume(CreateConsumeContext(message));

        _orderRepository.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static BasketCheckoutIntegrationEvent CreateMessage(Guid productId, int quantity) => new()
    {
        BuyerId = Guid.NewGuid().ToString(),
        Street = "1 Test Street",
        City = "Testville",
        State = "TS",
        Country = "Testland",
        ZipCode = "12345",
        Items = [new BasketCheckoutItem { ProductId = productId, Quantity = quantity }]
    };

    private static ConsumeContext<T> CreateConsumeContext<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
