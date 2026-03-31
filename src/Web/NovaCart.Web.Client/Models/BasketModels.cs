namespace NovaCart.Web.Client.Models;

public sealed class BasketModel
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemModel> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

public sealed class BasketItemModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public sealed class CheckoutModel
{
    public string BuyerId { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}
