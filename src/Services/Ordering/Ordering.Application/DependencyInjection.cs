using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
