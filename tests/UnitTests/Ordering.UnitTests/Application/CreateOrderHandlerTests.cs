using FluentAssertions;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Abstractions;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private static readonly AddressDto Address = new("123 Main St", "Springfield", "IL", "US", "62704");

    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxEventCollector _outboxEventCollector = Substitute.For<IOutboxEventCollector>();
    private readonly ICatalogProductReader _catalog = Substitute.For<ICatalogProductReader>();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_orderRepository, _unitOfWork, _outboxEventCollector, _catalog);
    }

    private void SetupCatalog(params CatalogProduct[] products)
    {
        IReadOnlyDictionary<Guid, CatalogProduct> map = products.ToDictionary(p => p.Id);
        _catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(map);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessWithId_When_ValidCommand()
    {
        var laptop = new CatalogProduct(Guid.NewGuid(), "Laptop", 999.99m, "USD");
        SetupCatalog(laptop);

        var command = new CreateOrderCommand(Guid.NewGuid(), Address, [new CreateOrderItemRequest(laptop.Id, 1)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _orderRepository.Received(1).Add(Arg.Any<Order>());
        _outboxEventCollector.Received(1).Add(Arg.Any<OrderCreatedIntegrationEvent>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PriceItemsFromCatalog_IgnoringClientInput()
    {
        var laptop = new CatalogProduct(Guid.NewGuid(), "Laptop", 999.99m, "USD");
        var mouse = new CatalogProduct(Guid.NewGuid(), "Mouse", 29.99m, "USD");
        SetupCatalog(laptop, mouse);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Address,
            [new CreateOrderItemRequest(laptop.Id, 1), new CreateOrderItemRequest(mouse.Id, 2)]);

        Order? captured = null;
        _orderRepository.When(r => r.Add(Arg.Any<Order>())).Do(ci => captured = ci.Arg<Order>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Items.Should().HaveCount(2);
        captured.TotalAmount.Should().Be(999.99m + 29.99m * 2);
    }

    [Fact]
    public async Task Handle_Should_PublishOrderCreatedEvent_With_CorrectCorrelationId()
    {
        var laptop = new CatalogProduct(Guid.NewGuid(), "Laptop", 999.99m, "USD");
        SetupCatalog(laptop);

        var command = new CreateOrderCommand(Guid.NewGuid(), Address, [new CreateOrderItemRequest(laptop.Id, 1)]);

        OrderCreatedIntegrationEvent? capturedEvent = null;
        _outboxEventCollector.When(c => c.Add(Arg.Any<IntegrationEvent>()))
            .Do(ci => capturedEvent = ci.Arg<IntegrationEvent>() as OrderCreatedIntegrationEvent);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedEvent.Should().NotBeNull();
        capturedEvent!.OrderId.Should().Be(result.Value);
        capturedEvent.BuyerId.Should().Be(command.BuyerId);
        capturedEvent.TotalAmount.Should().Be(999.99m);
        capturedEvent.CorrelationId.Should().Be(result.Value);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_ProductNotInCatalog()
    {
        SetupCatalog(); // empty — product unknown or not active

        var command = new CreateOrderCommand(Guid.NewGuid(), Address, [new CreateOrderItemRequest(Guid.NewGuid(), 1)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Order.UnknownProduct");
        _orderRepository.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
