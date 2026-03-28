using Microsoft.Extensions.DependencyInjection;
using NovaCart.Services.Basket.Domain.Repositories;
using NovaCart.Services.Basket.Infrastructure.Repositories;

namespace NovaCart.Services.Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IBasketRepository, RedisBasketRepository>();

        return services;
    }
}
