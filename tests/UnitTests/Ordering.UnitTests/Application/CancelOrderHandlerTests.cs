using FluentAssertions;
using NSubstitute;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class CancelOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelOrderHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_OrderCanBeCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Address.Create("St", "City", "State", "US", "12345"));
        var command = new CancelOrderCommand(order.Id);

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepository.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_OrderDoesNotExist()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.NewGuid());

        _orderRepository.GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_OrderIsDelivered()
    {
        // Arrange
        var order = CreateDeliveredOrder();
        var command = new CancelOrderCommand(order.Id);

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    private static Order CreateDeliveredOrder()
    {
        var order = Order.Create(Guid.NewGuid(), Address.Create("St", "City", "State", "US", "12345"));
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        order.Confirm();
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();
        return order;
    }
}
