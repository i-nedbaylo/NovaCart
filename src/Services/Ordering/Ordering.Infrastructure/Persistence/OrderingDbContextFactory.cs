using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCart.Services.Ordering.Infrastructure.Persistence;

// NOTE: Simplified for demo purposes. In production, read the connection string from
// environment variables or a secure configuration provider instead of hardcoding credentials.
// This factory is used only by EF Core CLI tools (dotnet ef migrations) at design time
// and is never invoked at runtime — Aspire provides connection strings dynamically.
public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? "Host=localhost;Database=orderingdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OrderingDbContext(optionsBuilder.Options);
    }
}
