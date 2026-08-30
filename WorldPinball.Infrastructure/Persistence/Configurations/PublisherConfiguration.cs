

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {

        public void Configure(EntityTypeBuilder<Publisher> e)
        {

            // =========================
            // publishers
            // =========================

            e.ToTable("publishers");
            e.HasKey(x => x.PublisherId);

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Slug).IsRequired();

            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.Slug).IsUnique();


        }
    }
}
