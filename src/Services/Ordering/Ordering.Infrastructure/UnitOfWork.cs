using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Infrastructure.Persistence;

namespace NovaCart.Services.Ordering.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderingDbContext _dbContext;

    public UnitOfWork(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
