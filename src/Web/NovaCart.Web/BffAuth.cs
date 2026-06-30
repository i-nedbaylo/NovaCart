using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.JsonWebTokens;
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

    public static IServiceCollection AddBffAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                // NOTE: Simplified for demo purposes. Aligned to the access-token lifetime;
                // refresh-token rotation is not wired into the BFF yet.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = "NovaCart.Auth";
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
            new(RefreshTokenClaim, token.RefreshToken)
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
