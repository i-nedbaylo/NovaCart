using System.Net;
using System.Net.Http.Json;
using NovaCart.Web.Client.Models;

namespace NovaCart.Web.Client.Services;

/// <summary>
/// Client service for basket operations. Used by InteractiveAuto components.
/// On Server: HttpClient points to Gateway (direct).
/// On WASM: HttpClient points to origin (BFF proxy → Gateway).
/// </summary>
public sealed class BasketClientService(HttpClient httpClient)
{
    // NOTE: Simplified for demo purposes. In production, buyer ID comes from authenticated user context.
    public const string DemoBuyerId = "00000000-0000-0000-0000-000000000001";

    public async Task<BasketModel?> GetBasketAsync(string buyerId, CancellationToken ct = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<BasketModel>($"/api/v1/baskets/{buyerId}", ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<BasketModel?> UpdateBasketAsync(
        string buyerId,
        List<BasketItemModel> items,
        CancellationToken ct = default)
    {
        var request = new { Items = items };
        var response = await httpClient.PutAsJsonAsync($"/api/v1/baskets/{buyerId}", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketModel>(ct);
    }

    public async Task DeleteBasketAsync(string buyerId, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/baskets/{buyerId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CheckoutAsync(CheckoutModel model, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/baskets/checkout", model, ct);
        response.EnsureSuccessStatusCode();
    }
}
