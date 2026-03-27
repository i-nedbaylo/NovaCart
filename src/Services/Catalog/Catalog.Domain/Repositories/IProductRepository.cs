using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Entities;

namespace NovaCart.Services.Catalog.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
}
