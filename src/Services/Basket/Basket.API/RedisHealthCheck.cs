using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NovaCart.Services.Basket.API;

/// <summary>
/// Readiness check that confirms Redis is actually reachable by performing a round-trip through the
/// configured <see cref="IDistributedCache"/> (the cart store). Keeps basket-api Unhealthy — so the
/// orchestrator and dependents (WaitFor) gate on it — until the cart store is usable, instead of
/// reporting Healthy on a self-only check while the first cart write would hang.
/// </summary>
public sealed class RedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.GetAsync("__health__", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is not reachable.", ex);
        }
    }
}
