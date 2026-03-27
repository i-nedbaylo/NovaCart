using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Identity.Application.Interfaces;
using NovaCart.Services.Identity.Domain.Entities;
using NovaCart.Services.Identity.Infrastructure.Persistence;

namespace NovaCart.Services.Identity.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityAppDbContext _dbContext;

    public UserRepository(IdentityAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);
    }
}
