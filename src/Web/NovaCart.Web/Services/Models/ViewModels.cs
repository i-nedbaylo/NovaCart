namespace NovaCart.Web.Services.Models;

public sealed record ProductViewModel(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    string? ImageUrl,
    decimal PriceAmount,
    string PriceCurrency,
    string Status,
    Guid CategoryId,
    string? CategoryName);

public sealed record CategoryViewModel(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    Guid? ParentCategoryId);

public sealed record OrderViewModel(
    Guid Id,
    string BuyerId,
    DateTimeOffset OrderDate,
    string Status,
    OrderAddressViewModel? ShippingAddress,
    List<OrderItemViewModel> Items,
    decimal TotalAmount);

public sealed record OrderAddressViewModel(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public sealed record OrderItemViewModel(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);

public sealed record LoginModel(string Email, string Password);

public sealed record RegisterModel(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public sealed class BasketViewModel
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemViewModel> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

public sealed class BasketItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
