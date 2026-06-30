using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.Outbox;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Infrastructure.Persistence;
using NovaCart.Services.Ordering.Infrastructure.Repositories;

namespace NovaCart.Services.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddOutbox<OrderingDbContext>();

        services.AddDbContext<OrderingDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<OutboxInterceptor>());
        });

        // Readiness: report Unhealthy until the database is reachable, so the orchestrator and
        // dependents (WaitFor) gate on it instead of racing a not-yet-ready service.
        services.AddHealthChecks()
            .AddDbContextCheck<OrderingDbContext>("orderingdb");

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
