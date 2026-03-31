using FluentAssertions;
using MassTransit;
using NSubstitute;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Tests.Basket.UnitTests.Application;

public class CheckoutBasketHandlerTests
{
    private readonly IBasketRepository _basketRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CheckoutBasketHandler _handler;

    public CheckoutBasketHandlerTests()
    {
        _basketRepository = Substitute.For<IBasketRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _handler = new CheckoutBasketHandler(_basketRepository, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ValidCheckout()
    {
        // Arrange
        var buyerId = "buyer-1";
        var cart = ShoppingCart.Create(buyerId);
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CheckoutBasketCommand(buyerId, "123 Main St", "Springfield", "IL", "US", "62704");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpoint.Received(1).Publish(
            Arg.Any<BasketCheckoutIntegrationEvent>(),
            Arg.Any<CancellationToken>());
        await _basketRepository.Received(1).DeleteBasketAsync(buyerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishEventWithCorrectData_When_ValidCheckout()
    {
        // Arrange
        var buyerId = "buyer-1";
        var productId = Guid.NewGuid();
        var cart = ShoppingCart.Create(buyerId);
        cart.AddItem(productId, "Laptop", 999.99m, 2);

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        BasketCheckoutIntegrationEvent? capturedEvent = null;
        await _publishEndpoint.Publish(
            Arg.Do<BasketCheckoutIntegrationEvent>(e => capturedEvent = e),
            Arg.Any<CancellationToken>());

        var command = new CheckoutBasketCommand(buyerId, "123 Main St", "Springfield", "IL", "US", "62704");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.BuyerId.Should().Be(buyerId);
        capturedEvent.TotalPrice.Should().Be(999.99m * 2);
        capturedEvent.Street.Should().Be("123 Main St");
        capturedEvent.City.Should().Be("Springfield");
        capturedEvent.Items.Should().ContainSingle();
        capturedEvent.Items[0].ProductId.Should().Be(productId);
        capturedEvent.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_BasketNotExists()
    {
        // Arrange
        var buyerId = "buyer-1";

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns((ShoppingCart?)null);

        var command = new CheckoutBasketCommand(buyerId, "123 Main St", "Springfield", "IL", "US", "62704");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("NotFound");
        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<BasketCheckoutIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_BasketIsEmpty()
    {
        // Arrange
        var buyerId = "buyer-1";
        var cart = ShoppingCart.Create(buyerId);

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CheckoutBasketCommand(buyerId, "123 Main St", "Springfield", "IL", "US", "62704");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Empty");
        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<BasketCheckoutIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }
}
