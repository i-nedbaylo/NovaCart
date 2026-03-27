using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
