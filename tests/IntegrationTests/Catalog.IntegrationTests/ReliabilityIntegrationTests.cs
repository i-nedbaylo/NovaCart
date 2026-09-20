using System.Text.Json;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NovaCart.BuildingBlocks.Outbox;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Infrastructure.Repositories;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Ordering.Infrastructure.Persistence;
using StackExchange.Redis;
using Order = NovaCart.Services.Ordering.Domain.Entities.Order;
using Testcontainers.PostgreSql;

namespace NovaCart.Tests.Catalog.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class ReliabilityIntegrationTests
{
    [Fact]
    public async Task RedisCheckout_IsAtomicIdempotent_AndDoesNotDeleteANewerCart()
    {
        await using var container = new ContainerBuilder("redis:7.4-alpine")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379)).Build();
        await container.StartAsync();
        var connection = $"{container.Hostname}:{container.GetMappedPublicPort(6379)}";
        await using var redis = await ConnectionMultiplexer.ConnectAsync(connection);
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(o => o.Configuration = connection);
        await using var provider = services.BuildServiceProvider();
        var baskets = new RedisBasketRepository(provider.GetRequiredService<IDistributedCache>());
        var store = new RedisCheckoutStore(redis);
        var cart = ShoppingCart.Create(Guid.NewGuid().ToString());
        cart.AddItem(Guid.NewGuid(), "Product", 10, 1);
        await baskets.UpdateBasketAsync(cart);
        var message = new BasketCheckoutIntegrationEvent { Id = cart.Revision, BuyerId = cart.BuyerId, PricingVersion = 1 };
        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => store.TryAcceptAsync(cart, message, default)));
        results.Should().OnlyContain(x => x);
        (await redis.GetDatabase().HashLengthAsync("checkout:pending")).Should().Be(1);
        (await baskets.GetBasketAsync(cart.BuyerId)).Should().BeNull();

        var next = ShoppingCart.Create(cart.BuyerId);
        next.AddItem(Guid.NewGuid(), "New product", 20, 1);
        await baskets.UpdateBasketAsync(next);
        (await store.TryAcceptAsync(cart, message, default)).Should().BeTrue();
        (await baskets.GetBasketAsync(cart.BuyerId))!.Revision.Should().Be(next.Revision);

        var stale = ShoppingCart.Create(cart.BuyerId);
        (await store.TryAcceptAsync(stale, message with { Id = stale.Revision }, default)).Should().BeFalse();
        (await baskets.GetBasketAsync(cart.BuyerId))!.Revision.Should().Be(next.Revision);
        // Acceptance survives a fresh store instance; no in-process idempotency cache is used.
        (await new RedisCheckoutStore(redis).IsAcceptedAsync(cart.BuyerId, cart.Revision, default)).Should().BeTrue();
    }

    [Fact]
    public async Task PostgreSql_RejectsPaymentOverwriteOfAcceptedCancellation_AndRetainsCurrency()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<OrderingDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var setup = new OrderingDbContext(options);
        await setup.Database.MigrateAsync();
        var order = Order.Create(Guid.NewGuid(), Address.Create("Street", "City", "State", "Country", "12345"), currency: "EUR");
        order.AddItem(Guid.NewGuid(), "Product", 10, 1);
        order.Confirm();
        setup.Orders.Add(order);
        await setup.SaveChangesAsync();

        await using var cancelling = new OrderingDbContext(options);
        await using var paying = new OrderingDbContext(options);
        var cancelCopy = await cancelling.Orders.SingleAsync();
        var payCopy = await paying.Orders.SingleAsync();
        cancelCopy.RequestCancellation();
        await cancelling.SaveChangesAsync();
        payCopy.MarkAsPaid();
        var save = () => paying.SaveChangesAsync();
        await save.Should().ThrowAsync<DbUpdateConcurrencyException>();
        await using var verify = new OrderingDbContext(options);
        var persisted = await verify.Orders.SingleAsync();
        persisted.Status.Should().Be(OrderStatus.CancellationPending);
        persisted.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Outbox_RecoversAfterMoreThanFivePublishFailures()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync();
        var services = new ServiceCollection();
        services.AddDbContext<OrderingDbContext>(o => o.UseNpgsql(postgres.GetConnectionString()));
        var publisher = Substitute.For<IPublishEndpoint>();
        var attempts = 0;
        publisher.Publish(Arg.Any<object>(), Arg.Any<Type>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (Interlocked.Increment(ref attempts) <= 6) throw new HttpRequestException("Simulated broker outage");
            return Task.CompletedTask;
        });
        services.AddSingleton(publisher);
        await using var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            await db.Database.MigrateAsync();
            var message = new OrderCreatedIntegrationEvent { OrderId = Guid.NewGuid(), TotalAmount = 10, Currency = "USD" };
            db.Set<OutboxMessage>().Add(OutboxMessage.Create(
                $"{message.GetType().FullName}, {message.GetType().Assembly.GetName().Name}",
                JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
            await db.SaveChangesAsync();
        }
        using var processor = new OutboxProcessor<OrderingDbContext>(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxOptions { PollingInterval = TimeSpan.FromMilliseconds(20), MaxRetries = 5 }),
            NullLogger<OutboxProcessor<OrderingDbContext>>.Instance);
        await processor.StartAsync(default);
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline)
            {
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                var row = await db.Set<OutboxMessage>().SingleAsync();
                if (row.ProcessedAt is not null)
                {
                    row.Error.Should().BeNull();
                    row.RetryCount.Should().Be(6);
                    attempts.Should().BeGreaterThanOrEqualTo(7);
                    return;
                }
                await Task.Delay(30);
            }
            throw new TimeoutException("Outbox did not recover.");
        }
        finally { await processor.StopAsync(default); }
    }
}
