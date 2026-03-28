using System.Text.Json.Serialization;

namespace NovaCart.Services.Basket.Domain.Entities;

public sealed class BasketItem
{
    [JsonInclude]
    public Guid ProductId { get; private set; }

    [JsonInclude]
    public string ProductName { get; private set; } = null!;

    [JsonInclude]
    public decimal Price { get; private set; }

    [JsonInclude]
    public int Quantity { get; private set; }

    public decimal TotalPrice => Price * Quantity;

    [JsonConstructor]
    private BasketItem() { }

    public static BasketItem Create(Guid productId, string productName, decimal price, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new BasketItem
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            Quantity = quantity
        };
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));

        Price = newPrice;
    }
}
