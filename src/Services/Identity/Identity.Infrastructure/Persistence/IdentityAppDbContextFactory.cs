using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaCart.Services.Identity.Infrastructure.Persistence;

// NOTE: Simplified for demo purposes. In production, read the connection string from
// environment variables or a secure configuration provider instead of hardcoding credentials.
// This factory is used only by EF Core CLI tools (dotnet ef migrations) at design time
// and is never invoked at runtime — Aspire provides connection strings dynamically.
public sealed class IdentityAppDbContextFactory : IDesignTimeDbContextFactory<IdentityAppDbContext>
{
    public IdentityAppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? "Host=localhost;Database=identitydb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityAppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new IdentityAppDbContext(optionsBuilder.Options);
    }
}
