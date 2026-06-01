using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicShop.Domain.Entities.Messaging;

namespace MusicShop.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Index on Status for quick lookup of PENDING/FAILED messages
        builder.HasIndex(message => message.Status);

        // Index on CreatedAt to preserve ordering and assist in age-cutoff scans
        builder.HasIndex(message => message.CreatedAt);
    }
}
