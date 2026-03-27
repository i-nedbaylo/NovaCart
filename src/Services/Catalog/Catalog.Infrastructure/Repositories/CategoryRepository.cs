using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Repositories;
using NovaCart.Services.Catalog.Infrastructure.Persistence;

namespace NovaCart.Services.Catalog.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public void Add(Category entity)
    {
        _dbContext.Categories.Add(entity);
    }

    public void Update(Category entity)
    {
        _dbContext.Categories.Update(entity);
    }

    public void Delete(Category entity)
    {
        _dbContext.Categories.Remove(entity);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<List<Category>> GetByParentIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
