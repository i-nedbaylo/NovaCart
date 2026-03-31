using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.BuildingBlocks.EventBus;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// Extension methods for registering Outbox Pattern services.
/// </summary>
public static class OutboxExtensions
{
    /// <summary>
    /// Registers the Outbox Pattern services: <see cref="IOutboxEventCollector"/> (scoped),
    /// <see cref="OutboxInterceptor"/> (scoped), and <see cref="OutboxProcessor{TDbContext}"/> (hosted service).
    /// Call this BEFORE <c>AddDbContext</c> and configure the interceptor in AddDbContext:
    /// <code>
    /// services.AddOutbox&lt;MyDbContext&gt;();
    /// services.AddDbContext&lt;MyDbContext&gt;((sp, options) =&gt; {
    ///     options.UseNpgsql(connectionString);
    ///     options.AddInterceptors(sp.GetRequiredService&lt;OutboxInterceptor&gt;());
    /// });
    /// </code>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to override default <see cref="OutboxOptions"/>.</param>
    public static IServiceCollection AddOutbox<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        var options = new OutboxOptions();
        configure?.Invoke(options);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.BatchSize, 0, nameof(options.BatchSize));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.MaxRetries, 0, nameof(options.MaxRetries));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.PollingInterval, TimeSpan.Zero, nameof(options.PollingInterval));

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));

        services.AddScoped<IOutboxEventCollector, OutboxEventCollector>();
        services.AddScoped<OutboxInterceptor>();
        services.AddHostedService<OutboxProcessor<TDbContext>>();

        return services;
    }

    /// <summary>
    /// Applies the <see cref="OutboxMessageConfiguration"/> to the model builder.
    /// Call this in <c>OnModelCreating</c> of each DbContext that uses the Outbox Pattern.
    /// </summary>
    public static ModelBuilder ApplyOutboxConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }
}
