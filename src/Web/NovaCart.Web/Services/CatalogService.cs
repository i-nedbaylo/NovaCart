using System.Net.Http.Json;
using NovaCart.Web.Services.Models;

namespace NovaCart.Web.Services;

public sealed class CatalogService
{
    private readonly HttpClient _httpClient;

    public CatalogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<ProductViewModel>?> GetProductsAsync(
        int page = 1,
        int pageSize = 10,
        Guid? categoryId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v1/products?pageNumber={page}&pageSize={pageSize}";

        if (categoryId.HasValue)
        {
            url += $"&categoryId={categoryId.Value}";
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        }

        return await _httpClient.GetFromJsonAsync<PagedResult<ProductViewModel>>(url, cancellationToken);
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ProductViewModel>($"/api/v1/products/{id}", cancellationToken);
    }

    public async Task<List<CategoryViewModel>?> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("/api/v1/categories", cancellationToken);
    }

    public async Task<CategoryViewModel?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<CategoryViewModel>($"/api/v1/categories/{id}", cancellationToken);
    }
}
