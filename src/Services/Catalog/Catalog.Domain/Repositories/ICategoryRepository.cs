using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Entities;

namespace NovaCart.Services.Catalog.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByParentIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}
