using System.Text.Json.Serialization;

namespace NovaCart.Services.Basket.Domain.Entities;

/// <summary>
/// Shopping cart aggregate. Stored in Redis, not in a relational database.
/// Does not inherit from AggregateRoot because it is not persisted via EF Core.
/// </summary>
public sealed class ShoppingCart
{
    [JsonInclude]
    public string BuyerId { get; private set; } = null!;

    [JsonInclude]
    public List<BasketItem> Items { get; private set; } = [];

    public decimal TotalPrice => Items.Sum(i => i.TotalPrice);

    [JsonConstructor]
    private ShoppingCart() { }

    public static ShoppingCart Create(string buyerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buyerId);

        return new ShoppingCart
        {
            BuyerId = buyerId
        };
    }

    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = BasketItem.Create(productId, productName, price, quantity);
            Items.Add(item);
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            Items.Remove(item);
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"Item with product ID '{productId}' not found in basket.");

        item.UpdateQuantity(quantity);
    }

    public void Clear()
    {
        Items.Clear();
    }
}
