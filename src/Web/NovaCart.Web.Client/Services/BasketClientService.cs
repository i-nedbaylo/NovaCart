using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NovaCart.Web.Client.Models;

namespace NovaCart.Web.Client.Services;

/// <summary>
/// Client service for basket operations. Used by InteractiveAuto components.
/// On Server: HttpClient points to the Gateway; the access token is attached from the cookie
/// principal via <see cref="IAccessTokenAccessor"/>.
/// On WASM: HttpClient points to origin and the BFF proxy attaches the token (the accessor
/// returns null here).
/// </summary>
public sealed class BasketClientService(HttpClient httpClient, IAccessTokenAccessor tokenAccessor)
{
    public async Task<BasketModel?> GetBasketAsync(string buyerId, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);

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
        await AuthorizeAsync(ct);

        var request = new { Items = items };
        var response = await httpClient.PutAsJsonAsync($"/api/v1/baskets/{buyerId}", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketModel>(ct);
    }

    public async Task DeleteBasketAsync(string buyerId, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);

        var response = await httpClient.DeleteAsync($"/api/v1/baskets/{buyerId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CheckoutAsync(CheckoutModel model, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);

        var response = await httpClient.PostAsJsonAsync("/api/v1/baskets/checkout", model, ct);
        response.EnsureSuccessStatusCode();
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
