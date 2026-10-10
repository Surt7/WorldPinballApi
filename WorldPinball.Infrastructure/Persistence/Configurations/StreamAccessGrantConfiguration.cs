using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public class StreamAccessGrantConfiguration : IEntityTypeConfiguration<StreamAccessGrant>
{
    public void Configure(EntityTypeBuilder<StreamAccessGrant> builder)
    {
        builder.ToTable("stream_access_grants");
        builder.HasKey(x => x.GrantId);

        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RevocationReason).HasMaxLength(500);

        // Fast lookup index to verify if a user or organization has stream rights
        builder.HasIndex(x => new { x.PrincipalType, x.PrincipalId });
        builder.HasIndex(x => x.Status);
    }
}