using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.Services.Identity.Domain;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Infrastructure.Persistence;

public static class IdentityDbContextSeed
{
    public static readonly string AdminRoleId = "a1b2c3d4-0002-0001-0001-000000000001";
    public static readonly string CustomerRoleId = "a1b2c3d4-0002-0001-0001-000000000002";

    // Fixed concurrency stamps. IdentityRole's constructor assigns a random Guid to
    // ConcurrencyStamp, so omitting it makes the seeded model differ from the migration snapshot
    // on every build — EF Core then reports pending model changes and MigrateAsync() throws
    // PendingModelChangesWarning, crashing the service against a fresh database. These values
    // must match the InitialCreate migration / snapshot.
    private const string AdminRoleConcurrencyStamp = "5d2315cd-bafe-42f7-8007-d8b2111fac53";
    private const string CustomerRoleConcurrencyStamp = "c893fd48-756b-411b-9453-b588a77d608c";

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = AdminRoleId,
                Name = UserRoles.Admin,
                NormalizedName = UserRoles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = AdminRoleConcurrencyStamp
            },
            new IdentityRole
            {
                Id = CustomerRoleId,
                Name = UserRoles.Customer,
                NormalizedName = UserRoles.Customer.ToUpperInvariant(),
                ConcurrencyStamp = CustomerRoleConcurrencyStamp
            });
    }

    // Admin credentials come from configuration, not source: the AppHost injects the password
    // (env var AdminUser__Password); it is never hardcoded here. Run the service standalone by
    // setting AdminUser:Password via user-secrets or an environment variable.
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var adminEmail = configuration["AdminUser:Email"] ?? "admin@novacart.com";
        var adminPassword = configuration["AdminUser:Password"]
            ?? throw new InvalidOperationException(
                "Admin seed password is not configured. Set 'AdminUser:Password' (the AppHost injects it).");

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = ApplicationUser.Create(adminEmail, "Admin", "NovaCart");
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }
    }
}
