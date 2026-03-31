using System.Net;
using System.Net.Http.Json;
using NovaCart.Web.Services.Models;

namespace NovaCart.Web.Services;

/// <summary>
/// Server-side BFF service for basket operations.
/// Used by SSR pages and server-rendered components.
/// </summary>
public sealed class BasketService
{
    private readonly HttpClient _httpClient;

    public BasketService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BasketViewModel?> GetBasketAsync(
        string buyerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BasketViewModel>(
                $"/api/v1/baskets/{buyerId}", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
