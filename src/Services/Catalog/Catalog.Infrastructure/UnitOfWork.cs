using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Infrastructure.Persistence;

namespace NovaCart.Services.Catalog.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _dbContext;

    public UnitOfWork(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
