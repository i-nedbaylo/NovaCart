using NovaCart.BuildingBlocks.Common;
using NovaCart.Services.Ordering.Domain.Events;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Services.Ordering.Domain.Entities;

public sealed class Order : AggregateRoot
{
    public Guid BuyerId { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];

    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    private Order() { }

    public static Order Create(Guid buyerId, Address shippingAddress)
    {
        if (buyerId == Guid.Empty)
            throw new ArgumentException("Buyer ID cannot be empty.", nameof(buyerId));

        var order = new Order
        {
            BuyerId = buyerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Created,
            ShippingAddress = shippingAddress
        };

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id));

        return order;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = OrderItem.Create(productId, productName, unitPrice, quantity);
            _items.Add(item);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order.");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        var oldStatus = Status.ToString();
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));
        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status.ToString()));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException($"Cannot confirm an order with status '{Status}'.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order with no items.");

        var oldStatus = Status.ToString();
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status.ToString()));
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot mark as paid an order with status '{Status}'.");

        var oldStatus = Status.ToString();
        Status = OrderStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status.ToString()));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot ship an order with status '{Status}'.");

        var oldStatus = Status.ToString();
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status.ToString()));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver an order with status '{Status}'.");

        var oldStatus = Status.ToString();
        Status = OrderStatus.Delivered;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status.ToString()));
    }
}
