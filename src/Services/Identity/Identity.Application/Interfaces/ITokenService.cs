using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    DateTimeOffset GetAccessTokenExpiration();
}
