using Microsoft.EntityFrameworkCore;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Payment.Infrastructure.Persistence;

namespace NovaCart.Services.Payment.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _dbContext;
    private readonly IOutboxEventCollector _events;

    public UnitOfWork(PaymentDbContext dbContext, IOutboxEventCollector events)
    {
        _dbContext = dbContext;
        _events = events;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { return await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            // A message retry must re-read the winner's state, not reuse stale tracked entities
            // or publish events collected by the rolled-back attempt.
            _dbContext.ChangeTracker.Clear();
            _events.Clear();
            throw;
        }
    }
}
