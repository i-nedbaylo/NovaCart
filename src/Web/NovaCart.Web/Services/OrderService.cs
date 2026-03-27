using System.Net.Http.Json;
using NovaCart.Web.Services.Models;

namespace NovaCart.Web.Services;

public sealed class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<OrderViewModel>?> GetOrdersAsync(
        string? buyerId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v1/orders?pageNumber={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(buyerId))
        {
            url += $"&buyerId={buyerId}";
        }

        return await _httpClient.GetFromJsonAsync<PagedResult<OrderViewModel>>(url, cancellationToken);
    }

    public async Task<OrderViewModel?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OrderViewModel>($"/api/v1/orders/{id}", cancellationToken);
    }
}
