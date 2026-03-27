using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Infrastructure.Persistence;

namespace NovaCart.Services.Ordering.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _dbContext;

    public OrderRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order entity)
    {
        _dbContext.Orders.Add(entity);
    }

    public void Update(Order entity)
    {
        _dbContext.Orders.Update(entity);
    }

    public void Delete(Order entity)
    {
        _dbContext.Orders.Remove(entity);
    }

    public async Task<List<Order>> GetByBuyerIdAsync(Guid buyerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? buyerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (buyerId.HasValue)
            query = query.Where(o => o.BuyerId == buyerId.Value);

        query = query.OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
