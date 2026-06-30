using System.Net;
using System.Net.Http.Json;

namespace NovaCart.Web.Client.Services;

/// <summary>
/// WASM-side <see cref="IUserService"/>. Asks the BFF (/bff/user) for the current user; the
/// browser's auth cookie travels with the request, so the token stays on the server.
/// </summary>
public sealed class ClientUserService(HttpClient httpClient) : IUserService
{
    public async Task<BuyerInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("/bff/user", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BuyerInfo>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
