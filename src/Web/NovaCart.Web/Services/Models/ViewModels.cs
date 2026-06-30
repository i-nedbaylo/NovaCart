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
