using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace NovaCart.Web;

/// <summary>
/// Attaches the authenticated user's access token (held server-side in the cookie principal)
/// as a Bearer header on outbound Gateway calls made from server-rendered or interactive-server
/// components. WASM-origin calls go through <see cref="BffProxy"/>, which attaches the token there.
/// </summary>
public sealed class BffTokenHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BffTokenHandler(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var token = state.User.FindFirst(BffAuth.AccessTokenClaim)?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
