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

        // Readiness: report Unhealthy until the database is reachable, so the orchestrator and
        // dependents (WaitFor) gate on it instead of racing a not-yet-ready service.
        services.AddHealthChecks()
            .AddDbContextCheck<CatalogDbContext>("catalogdb");

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
