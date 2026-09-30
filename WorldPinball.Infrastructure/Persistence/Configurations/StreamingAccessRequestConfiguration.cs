using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public class StreamingAccessRequestConfiguration : IEntityTypeConfiguration<StreamingAccessRequest>
{
    public void Configure(EntityTypeBuilder<StreamingAccessRequest> builder)
    {
        builder.ToTable("streaming_access_requests");
        builder.HasKey(x => x.RequestId);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.HasIndex(x => x.SessionId);
    }
}