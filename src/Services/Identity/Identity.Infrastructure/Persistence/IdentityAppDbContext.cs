using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Infrastructure.Persistence;

public sealed class IdentityAppDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>().ToTable("users");
        modelBuilder.Entity<IdentityRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");

        IdentityDbContextSeed.Seed(modelBuilder);
    }
}
