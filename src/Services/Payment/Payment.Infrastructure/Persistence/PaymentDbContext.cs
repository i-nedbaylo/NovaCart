using Microsoft.EntityFrameworkCore;
using NovaCart.BuildingBlocks.Outbox;
using NovaCart.Services.Payment.Domain.Entities;

namespace NovaCart.Services.Payment.Infrastructure.Persistence;

public sealed class PaymentDbContext : DbContext
{
    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        modelBuilder.ApplyOutboxConfiguration();
    }
}
