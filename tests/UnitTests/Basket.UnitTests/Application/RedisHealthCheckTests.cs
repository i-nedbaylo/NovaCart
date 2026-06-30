using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NovaCart.Services.Basket.API;

namespace NovaCart.Tests.Basket.UnitTests.Application;

public class RedisHealthCheckTests
{
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();

    [Fact]
    public async Task CheckHealth_Should_Return_Healthy_When_Cache_Responds()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(null));
        var check = new RedisHealthCheck(_cache);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealth_Should_Return_Unhealthy_When_Cache_Throws()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<byte[]?>(new InvalidOperationException("redis down")));
        var check = new RedisHealthCheck(_cache);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
