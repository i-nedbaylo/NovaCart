using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Services.Payment.Infrastructure.Persistence.Configurations;

public sealed class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.HasIndex(p => p.OrderId)
            .IsUnique()
            .HasDatabaseName("ix_payments_order_id");

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.Value,
                v => PaymentStatus.From(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(p => p.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Optimistic concurrency via PostgreSQL xmin system column.
        // Prevents concurrent consumers from both transitioning a Pending payment.
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }
}
