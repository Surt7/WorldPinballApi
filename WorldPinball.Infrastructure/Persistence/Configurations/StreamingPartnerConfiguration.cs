using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public class StreamingPartnerConfiguration : IEntityTypeConfiguration<StreamingPartner>
{
    public void Configure(EntityTypeBuilder<StreamingPartner> builder)
    {
        builder.ToTable("streaming_partners");
        builder.HasKey(x => x.PartnerId);
        builder.Property(x => x.OrganizationName).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}