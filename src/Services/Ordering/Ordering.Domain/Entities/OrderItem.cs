using NovaCart.BuildingBlocks.Common;

namespace NovaCart.Services.Ordering.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Guid OrderId { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    internal static OrderItem Create(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
