using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicShop.Domain.Entities.Messaging;

namespace MusicShop.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(message => message.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(message => message.IdempotencyKey)
            .HasMaxLength(512);

        builder.Property(message => message.LockId)
            .HasMaxLength(64);

        builder.Property(message => message.Error)
            .HasMaxLength(2000);

        // ── outbox idempotency ─────────────────────────
        builder.HasIndex(message => message.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"ProcessedAt\" IS NULL");

        // ── fast polling query ─────────────────────────
        builder.HasIndex(message => message.ProcessedAt);
    }
}
