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

                // Resilience: retry a consumer in-process on transient failures (e.g. a brief
                // database or network hiccup) before the message is moved to the _error queue.
                // The Outbox guarantees delivery on the publish side; this hardens the consume
                // side. Consumers are idempotent (terminal-status guards / existing-record
                // checks), so re-running them on retry is safe.
                cfg.UseMessageRetry(retry => retry.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)));

                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
