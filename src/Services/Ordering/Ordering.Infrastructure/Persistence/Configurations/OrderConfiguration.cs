using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Services.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.BuyerId)
            .HasColumnName("buyer_id")
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .HasColumnName("order_date")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        builder.OwnsOne(o => o.ShippingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("shipping_street")
                .HasMaxLength(200)
                .IsRequired();

            addressBuilder.Property(a => a.City)
                .HasColumnName("shipping_city")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.State)
                .HasColumnName("shipping_state")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.Country)
                .HasColumnName("shipping_country")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("shipping_zip_code")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Navigation(o => o.ShippingAddress).IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
