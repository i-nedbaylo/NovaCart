using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Catalog.Domain.Entities;

namespace NovaCart.Services.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        CatalogDbContextSeed.Seed(modelBuilder);
    }
}
