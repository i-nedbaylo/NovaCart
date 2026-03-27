using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCart.Services.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=orderingdb;Username=postgres;Password=postgres");

        return new OrderingDbContext(optionsBuilder.Options);
    }
}
