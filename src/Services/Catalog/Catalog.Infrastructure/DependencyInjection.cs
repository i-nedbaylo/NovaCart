using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Domain.Repositories;
using NovaCart.Services.Catalog.Infrastructure.Persistence;
using NovaCart.Services.Catalog.Infrastructure.Repositories;

namespace NovaCart.Services.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
