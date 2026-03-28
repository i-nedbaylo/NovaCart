using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCart.BuildingBlocks.Outbox;

/// <summary>
/// EF Core configuration for the outbox_messages table.
/// Uses snake_case naming (PostgreSQL convention) and a filtered index
/// on unprocessed messages for efficient polling.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasMaxLength(2048);

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("ix_outbox_messages_unprocessed")
            .HasFilter("processed_at IS NULL");
    }
}
