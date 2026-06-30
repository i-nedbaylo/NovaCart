using Microsoft.AspNetCore.Components.Authorization;
using NovaCart.Web.Client.Services;

namespace NovaCart.Web;

/// <summary>
/// Server implementation: reads the access token from the cookie principal via the authentication
/// state. It is injected into the client services, which are resolved in the component/circuit
/// scope, so it sees the authenticated user on both prerender and Interactive Server. This is the
/// key difference from a <c>DelegatingHandler</c>: IHttpClientFactory resolves message handlers in
/// a separate, long-lived scope that does NOT carry the circuit's authentication state, so a
/// handler-based approach silently fails to attach the token on the server-render path.
/// </summary>
public sealed class ServerAccessTokenAccessor(AuthenticationStateProvider authStateProvider) : IAccessTokenAccessor
{
    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirst(BffAuth.AccessTokenClaim)?.Value;
    }
}
