using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NovaCart.BuildingBlocks.EventBus;

public static class EventBusExtensions
{
    /// <summary>
    /// Adds MassTransit with RabbitMQ transport to the host application builder.
    /// Automatically discovers and registers consumers from the specified assemblies.
    /// </summary>
    public static IHostApplicationBuilder AddEventBus(
        this IHostApplicationBuilder builder,
        params Assembly[] consumerAssemblies)
    {
        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            foreach (var assembly in consumerAssemblies)
            {
                config.AddConsumers(assembly);
            }

            config.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("rabbitmq")
                    ?? throw new InvalidOperationException(
                        "RabbitMQ connection string is missing. Please configure 'ConnectionStrings:rabbitmq' in application settings.");

                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
