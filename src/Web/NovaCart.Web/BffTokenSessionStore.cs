using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using NovaCart.Web.Services;
using NovaCart.Web.Services.Models;
namespace NovaCart.Web;

// One synchronized token chain is shared by cookie HTTP requests and server circuits.
// For multiple Web replicas replace this local store with a distributed session/token store.
public sealed class BffTokenSessionStore(IMemoryCache cache, IHttpClientFactory clients, TimeProvider clock)
{
    private readonly object _creationLock = new();
    private sealed class Session(TokenResponse token)
    {
        public readonly SemaphoreSlim Gate = new(1, 1);
        public TokenResponse Token = token;
        public bool Revoked;
    }

    public static string SessionId(ClaimsPrincipal principal) =>
        principal.FindFirst(BffAuth.SessionIdClaim)?.Value
        ?? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            principal.FindFirst(BffAuth.RefreshTokenClaim)?.Value ?? string.Empty)));

    public async Task<TokenResponse?> GetAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        var refresh = principal.FindFirst(BffAuth.RefreshTokenClaim)?.Value;
        var access = principal.FindFirst(BffAuth.AccessTokenClaim)?.Value;
        if (string.IsNullOrEmpty(refresh) || string.IsNullOrEmpty(access)
            || !long.TryParse(principal.FindFirst(BffAuth.ExpiresAtClaim)?.Value, out var expiry)) return null;

        var key = "bff-session:" + SessionId(principal);
        Session session;
        lock (_creationLock)
        {
            if (!cache.TryGetValue(key, out Session? found))
            {
                found = new Session(new TokenResponse(access, refresh, DateTimeOffset.FromUnixTimeSeconds(expiry)));
                cache.Set(key, found, TimeSpan.FromDays(7));
            }
            session = found!;
        }
        await session.Gate.WaitAsync(ct);
        try
        {
            if (session.Revoked) return null;
            if (clock.GetUtcNow() >= session.Token.ExpiresAt - TimeSpan.FromMinutes(5))
            {
                var service = new AuthService(clients.CreateClient("Gateway"));
                var renewed = await service.RefreshTokenAsync(session.Token.RefreshToken, ct);
                if (renewed is not null) session.Token = renewed;
            }
            return clock.GetUtcNow() < session.Token.ExpiresAt ? session.Token : null;
        }
        finally { session.Gate.Release(); }
    }

    public async Task RevokeAsync(ClaimsPrincipal principal)
    {
        var key = "bff-session:" + SessionId(principal);
        Session session;
        lock (_creationLock)
        {
            if (!cache.TryGetValue(key, out Session? found))
            {
                found = new Session(new TokenResponse("", "", DateTimeOffset.MinValue));
                cache.Set(key, found, TimeSpan.FromDays(7));
            }
            session = found!;
        }
        await session.Gate.WaitAsync();
        try { session.Revoked = true; }
        finally { session.Gate.Release(); }
    }
}
