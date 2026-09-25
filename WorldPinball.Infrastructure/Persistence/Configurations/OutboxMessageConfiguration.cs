using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration: IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(x => x.Content)
            .IsRequired();
        
        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();
        
        builder.Property(x => x.ProcessedAtUtc);
        
        builder.Property(x => x.Error);
        
        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.HasIndex(x => x.ProcessedAtUtc); // Index for the worker
    }
}