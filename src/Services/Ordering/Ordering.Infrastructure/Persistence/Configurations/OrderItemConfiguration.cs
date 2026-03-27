using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaCart.Services.Ordering.Domain.Entities;

namespace NovaCart.Services.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(i => i.TotalPrice);
    }
}
