using Microsoft.AspNetCore.Components.Authorization;
using NovaCart.Web.Client.Services;
namespace NovaCart.Web;

// Circuit actions do not run cookie middleware: resolve/renew the shared token here.
public sealed class ServerAccessTokenAccessor(AuthenticationStateProvider authStateProvider,
    BffTokenSessionStore sessions) : IAccessTokenAccessor
{
    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        return (await sessions.GetAsync(state.User, cancellationToken))?.AccessToken;
    }
}
