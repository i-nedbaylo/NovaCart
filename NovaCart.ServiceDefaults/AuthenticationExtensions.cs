using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace NovaCart.ServiceDefaults;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication using the shared "Jwt" configuration section
    /// (Secret, Issuer, Audience). Tokens are issued by the Identity service; every
    /// protected service validates them with the same parameters, so the gateway can
    /// stay a thin pass-through and each service enforces its own authorization.
    /// </summary>
    public static IHostApplicationBuilder AddJwtAuthentication(this IHostApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");

        var secret = jwtSection["Secret"]
            ?? throw new InvalidOperationException("JWT 'Secret' is not configured.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated user's id (from the NameIdentifier claim) as a Guid,
    /// or null if the claim is missing or malformed.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
