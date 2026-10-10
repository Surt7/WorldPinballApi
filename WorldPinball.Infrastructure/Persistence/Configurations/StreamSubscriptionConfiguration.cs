using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public class StreamSubscriptionConfiguration : IEntityTypeConfiguration<StreamSubscription>
{
    public void Configure(EntityTypeBuilder<StreamSubscription> builder)
    {
        builder.ToTable("stream_subscriptions");
        builder.HasKey(x => x.SubscriptionId);

        // Ensure a user cannot subscribe to the same target multiple times
        builder.HasIndex(x => new { x.SubscriberUserId, x.TargetType, x.TargetPrincipalId })
            .IsUnique();
    }
}