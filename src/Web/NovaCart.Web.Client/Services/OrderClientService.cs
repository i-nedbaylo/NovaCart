using System.Net.Http.Json;
using NovaCart.Web.Client.Models;

namespace NovaCart.Web.Client.Services;

/// <summary>
/// Client service for order operations. Used by InteractiveAuto components.
/// On Server: HttpClient points to Gateway (direct).
/// On WASM: HttpClient points to origin (BFF proxy → Gateway).
/// </summary>
public sealed class OrderClientService(HttpClient httpClient)
{
    public async Task<PagedResultModel<OrderModel>?> GetOrdersAsync(
        Guid? buyerId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var url = $"/api/v1/orders?pageNumber={page}&pageSize={pageSize}";

        if (buyerId.HasValue)
        {
            url += $"&buyerId={buyerId.Value}";
        }

        return await httpClient.GetFromJsonAsync<PagedResultModel<OrderModel>>(url, ct);
    }

    public async Task<OrderModel?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<OrderModel>($"/api/v1/orders/{id}", ct);
    }
}
