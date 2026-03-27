using FluentAssertions;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Events;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Tests.Ordering.UnitTests.Domain;

public class OrderTests
{
    private static Address CreateValidAddress() =>
        Address.Create("123 Main St", "Springfield", "IL", "US", "62704");

    [Fact]
    public void Create_Should_ReturnOrder_When_ValidData()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var address = CreateValidAddress();

        // Act
        var order = Order.Create(buyerId, address);

        // Assert
        order.Should().NotBeNull();
        order.BuyerId.Should().Be(buyerId);
        order.Status.Should().Be(OrderStatus.Created);
        order.ShippingAddress.Should().Be(address);
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0m);
        order.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_RaiseOrderCreatedDomainEvent_When_Created()
    {
        // Act
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_EmptyBuyerId()
    {
        // Act
        var act = () => Order.Create(Guid.Empty, CreateValidAddress());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("buyerId");
    }

    [Fact]
    public void AddItem_Should_AddNewItem_When_ProductNotInOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        var productId = Guid.NewGuid();

        // Act
        order.AddItem(productId, "Laptop", 999.99m, 1);

        // Assert
        order.Items.Should().ContainSingle();
        order.Items.First().ProductId.Should().Be(productId);
        order.Items.First().ProductName.Should().Be("Laptop");
        order.Items.First().UnitPrice.Should().Be(999.99m);
        order.Items.First().Quantity.Should().Be(1);
        order.TotalAmount.Should().Be(999.99m);
    }

    [Fact]
    public void AddItem_Should_MergeQuantity_When_SameProductAdded()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        var productId = Guid.NewGuid();

        // Act
        order.AddItem(productId, "Laptop", 999.99m, 1);
        order.AddItem(productId, "Laptop", 999.99m, 2);

        // Assert
        order.Items.Should().ContainSingle();
        order.Items.First().Quantity.Should().Be(3);
        order.TotalAmount.Should().Be(999.99m * 3);
    }

    [Fact]
    public void RemoveItem_Should_RemoveItem_When_ProductExists()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        var productId = Guid.NewGuid();
        order.AddItem(productId, "Laptop", 999.99m, 1);

        // Act
        order.RemoveItem(productId);

        // Assert
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Cancel_Should_SetStatusToCancelled_When_OrderIsCreated()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Should_RaiseDomainEvents_When_Cancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.ClearDomainEvents();

        // Act
        order.Cancel();

        // Assert
        order.DomainEvents.Should().HaveCount(2);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledDomainEvent);
        order.DomainEvents.Should().ContainSingle(e => e is OrderStatusChangedDomainEvent);
    }

    [Fact]
    public void Cancel_Should_ThrowInvalidOperationException_When_OrderIsDelivered()
    {
        // Arrange
        var order = CreateDeliveredOrder();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*delivered*");
    }

    [Fact]
    public void Cancel_Should_ThrowInvalidOperationException_When_OrderIsAlreadyCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.Cancel();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public void Confirm_Should_SetStatusToConfirmed_When_OrderHasItems()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_Should_ThrowInvalidOperationException_When_NoItems()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no items*");
    }

    [Fact]
    public void Confirm_Should_ThrowInvalidOperationException_When_NotCreatedStatus()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        order.Confirm();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FullLifecycle_Should_TransitionThroughAllStatuses_When_ValidFlow()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act & Assert
        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);

        order.MarkAsPaid();
        order.Status.Should().Be(OrderStatus.Paid);

        order.Ship();
        order.Status.Should().Be(OrderStatus.Shipped);

        order.Deliver();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void MarkAsPaid_Should_ThrowInvalidOperationException_When_NotConfirmed()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());

        // Act
        var act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ship_Should_ThrowInvalidOperationException_When_NotPaid()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        order.Confirm();

        // Act
        var act = () => order.Ship();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deliver_Should_ThrowInvalidOperationException_When_NotShipped()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        order.Confirm();
        order.MarkAsPaid();

        // Act
        var act = () => order.Deliver();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static Order CreateDeliveredOrder()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidAddress());
        order.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        order.Confirm();
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();
        return order;
    }
}
