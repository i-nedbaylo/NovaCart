using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Payment.Infrastructure.Persistence;

namespace NovaCart.Services.Payment.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _dbContext;

    public UnitOfWork(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
