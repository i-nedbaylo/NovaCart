using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaCart.Services.Identity.Domain;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Infrastructure.Persistence;

public static class IdentityDbContextSeed
{
    public static readonly string AdminRoleId = "a1b2c3d4-0002-0001-0001-000000000001";
    public static readonly string CustomerRoleId = "a1b2c3d4-0002-0001-0001-000000000002";

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
                NormalizedName = UserRoles.Admin.ToUpperInvariant()
            },
            new IdentityRole
            {
                Id = CustomerRoleId,
                Name = UserRoles.Customer,
                NormalizedName = UserRoles.Customer.ToUpperInvariant()
            });
    }

    // NOTE: Simplified for demo purposes. In production, admin user credentials should come
    // from a secure configuration provider (e.g., Azure Key Vault, environment variables).
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@novacart.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = ApplicationUser.Create(adminEmail, "Admin", "NovaCart");
        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }
    }
}
