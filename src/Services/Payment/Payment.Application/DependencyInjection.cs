using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Payment.Application.Options;

namespace NovaCart.Services.Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<PaymentSimulationOptions>(_ => { });

        return services;
    }
}
