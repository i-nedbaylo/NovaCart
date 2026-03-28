using Microsoft.EntityFrameworkCore;
using NovaCart.BuildingBlocks.Outbox;
using NovaCart.Services.Ordering.Domain.Entities;

namespace NovaCart.Services.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
        modelBuilder.ApplyOutboxConfiguration();
    }
}
