using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using NovaCart.Web.Services;
using NovaCart.Web.Services.Models;

namespace NovaCart.Web;

/// <summary>
/// Backend-for-Frontend authentication: the Blazor server host holds the Identity-issued
/// JWT inside an HttpOnly cookie and attaches it to downstream Gateway calls. The raw token
/// is never exposed to the browser/WASM (ADR-003).
/// </summary>
public static class BffAuth
{
    public const string AccessTokenClaim = "access_token";
    public const string RefreshTokenClaim = "refresh_token";
    public const string ExpiresAtClaim = "exp_at";

    // Refresh the access token this long before it expires so downstream calls never use an
    // already-expired token.
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(5);

    public static IServiceCollection AddBffAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = "NovaCart.Auth";

                // Silently refresh the access token before it expires, so an active session
                // outlives the 60-minute access-token lifetime (up to the 7-day refresh window).
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = RefreshAccessTokenAsync
                };
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        return services;
    }

    /// <summary>
    /// Builds a cookie principal from the Identity-issued JWT. Identity/role claims are copied
    /// from the token; the raw access/refresh tokens are kept as server-only claims so the BFF
    /// can attach them to downstream calls. These token claims are never sent to the browser.
    /// </summary>
    public static ClaimsPrincipal BuildPrincipal(TokenResponse token)
    {
        var jwt = new JsonWebToken(token.AccessToken);

        string? First(params string[] types) =>
            jwt.Claims.FirstOrDefault(c => types.Contains(c.Type))?.Value;

        var userId = First(ClaimTypes.NameIdentifier, "nameid", "sub") ?? string.Empty;
        var email = First(ClaimTypes.Email, "email") ?? string.Empty;
        var name = First(ClaimTypes.Name, "unique_name", "name") ?? email;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new(AccessTokenClaim, token.AccessToken),
            new(RefreshTokenClaim, token.RefreshToken),
            new(ExpiresAtClaim, token.ExpiresAt.ToUnixTimeSeconds().ToString())
        };

        claims.AddRange(jwt.Claims
            .Where(c => c.Type is ClaimTypes.Role or "role" or "roles")
            .Select(c => new Claim(ClaimTypes.Role, c.Value)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    // Cookie validation hook: when the access token is near (or past) expiry, exchange the
    // rotating refresh token for a fresh pair via Identity and re-issue the cookie.
    private static async Task RefreshAccessTokenAsync(CookieValidatePrincipalContext context)
    {
        var expiresAt = GetAccessTokenExpiry(context.Principal);
        if (expiresAt is null)
            return;

        var now = DateTimeOffset.UtcNow;
        if (now < expiresAt.Value - RefreshThreshold)
            return; // access token still fresh

        var refreshToken = context.Principal?.FindFirst(RefreshTokenClaim)?.Value;
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
            var refreshed = await authService.RefreshTokenAsync(refreshToken);
            if (refreshed is not null)
            {
                context.ReplacePrincipal(BuildPrincipal(refreshed));
                context.ShouldRenew = true;
                return;
            }
        }

        // Refresh unavailable or rejected — only end the session once the token is actually
        // expired, so a transient failure (or a rotation race with a concurrent request) never
        // signs the user out prematurely.
        if (now >= expiresAt.Value)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    private static DateTimeOffset? GetAccessTokenExpiry(ClaimsPrincipal? principal)
    {
        var value = principal?.FindFirst(ExpiresAtClaim)?.Value;
        return long.TryParse(value, out var unixSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : null;
    }

    public static WebApplication MapBffAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/bff");

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("/");
        }).DisableAntiforgery();

        // Returns the current user so WASM components can resolve the buyer id without ever
        // seeing the access token. Returns 401 (not a redirect) when unauthenticated.
        group.MapGet("/user", (HttpContext http) =>
        {
            var user = http.User;
            if (user.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            return Results.Ok(new BffUser(
                user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                user.FindFirstValue(ClaimTypes.Name) ?? string.Empty));
        });

        return app;
    }
}

public sealed record BffUser(string Id, string Email, string Name);
