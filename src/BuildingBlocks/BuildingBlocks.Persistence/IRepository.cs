using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.Persistence;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
