using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> e)
        {
            // =========================
            // regions
            // =========================

            e.ToTable("regions");
            e.HasKey(x => x.RegionId);

            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Name).IsRequired();

            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.Code).IsUnique();

        }
    }
}
