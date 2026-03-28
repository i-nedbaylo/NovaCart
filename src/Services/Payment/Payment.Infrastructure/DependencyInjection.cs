using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Infrastructure.Persistence;
using NovaCart.Services.Payment.Infrastructure.Repositories;

namespace NovaCart.Services.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
