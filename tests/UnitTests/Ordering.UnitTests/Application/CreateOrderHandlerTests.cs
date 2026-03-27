using FluentAssertions;
using NSubstitute;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateOrderHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessWithId_When_ValidCommand()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new AddressDto("123 Main St", "Springfield", "IL", "US", "62704"),
            [new CreateOrderItemRequest(Guid.NewGuid(), "Laptop", 999.99m, 1)]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _orderRepository.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CreateOrderWithCorrectItems_When_MultipleItems()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new AddressDto("123 Main St", "Springfield", "IL", "US", "62704"),
            [
                new CreateOrderItemRequest(Guid.NewGuid(), "Laptop", 999.99m, 1),
                new CreateOrderItemRequest(Guid.NewGuid(), "Mouse", 29.99m, 2)
            ]);

        Order? capturedOrder = null;
        _orderRepository.When(r => r.Add(Arg.Any<Order>()))
            .Do(ci => capturedOrder = ci.Arg<Order>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().HaveCount(2);
        capturedOrder.TotalAmount.Should().Be(999.99m + 29.99m * 2);
    }
}
