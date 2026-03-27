using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Infrastructure.Persistence;
using NovaCart.Services.Ordering.Infrastructure.Repositories;

namespace NovaCart.Services.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
