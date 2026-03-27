using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Entities;

namespace NovaCart.Services.Ordering.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetByBuyerIdAsync(Guid buyerId, CancellationToken cancellationToken = default);
    Task<(List<Order> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? buyerId = null,
        CancellationToken cancellationToken = default);
}
