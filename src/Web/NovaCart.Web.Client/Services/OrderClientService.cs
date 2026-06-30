using System.Net.Http.Headers;
using System.Net.Http.Json;
using NovaCart.Web.Client.Models;

namespace NovaCart.Web.Client.Services;

/// <summary>
/// Client service for order operations. Used by InteractiveAuto components.
/// On Server: HttpClient points to the Gateway; the access token is attached from the cookie
/// principal via <see cref="IAccessTokenAccessor"/>.
/// On WASM: HttpClient points to origin and the BFF proxy attaches the token (the accessor
/// returns null here).
/// </summary>
public sealed class OrderClientService(HttpClient httpClient, IAccessTokenAccessor tokenAccessor)
{
    public async Task<PagedResultModel<OrderModel>?> GetOrdersAsync(
        Guid? buyerId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);

        var url = $"/api/v1/orders?pageNumber={page}&pageSize={pageSize}";

        if (buyerId.HasValue)
        {
            url += $"&buyerId={buyerId.Value}";
        }

        return await httpClient.GetFromJsonAsync<PagedResultModel<OrderModel>>(url, ct);
    }

    public async Task<OrderModel?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);

        return await httpClient.GetFromJsonAsync<OrderModel>($"/api/v1/orders/{id}", ct);
    }

    // On the server the client targets the Gateway directly, so the user's token must be attached
    // here. On WASM the accessor returns null and the BFF proxy supplies the token instead.
    private async Task AuthorizeAsync(CancellationToken ct)
    {
        var token = await tokenAccessor.GetAccessTokenAsync(ct);
        httpClient.DefaultRequestHeaders.Authorization =
            token is null ? null : new AuthenticationHeaderValue("Bearer", token);
    }
}
