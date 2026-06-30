using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using NovaCart.Web.Client.Services;

namespace NovaCart.Web;

/// <summary>
/// Server-side <see cref="IUserService"/> used while interactive components render on the
/// Blazor server. The current user comes from the cookie principal via the authentication
/// state, so no extra HTTP round-trip is needed.
/// </summary>
public sealed class ServerUserService(AuthenticationStateProvider authStateProvider) : IUserService
{
    public async Task<BuyerInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        if (user.Identity?.IsAuthenticated != true)
            return null;

        return new BuyerInfo(
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Name) ?? string.Empty);
    }
}
