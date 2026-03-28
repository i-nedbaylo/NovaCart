using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.Outbox;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Infrastructure.Persistence;
using NovaCart.Services.Payment.Infrastructure.Repositories;

namespace NovaCart.Services.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddOutbox<PaymentDbContext>();

        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<OutboxInterceptor>());
        });

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
