using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class IpLicenseConfiguration : IEntityTypeConfiguration<IpLicense>
    {

        public void Configure(EntityTypeBuilder<IpLicense> e)
        {
            // =========================
            // ip_licenses
            // =========================

            e.ToTable("ip_licenses");
            e.HasKey(x => x.IpId);

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Slug).IsRequired();

            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.PublisherId);
            e.HasIndex(x => x.IsActive);

            e.HasOne(x => x.Publisher)
             .WithMany(x => x.IpLicenses)
             .HasForeignKey(x => x.PublisherId)
             .OnDelete(DeleteBehavior.Restrict);

            // ip_license_regions (PK composite)
            e.HasMany(x => x.Regions)
             .WithMany(x => x.IpLicenses)
             .UsingEntity<Dictionary<string, object>>(
                "ip_license_regions",
                j => j.HasOne<Region>()
                      .WithMany()
                      .HasForeignKey("region_id")
                      .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<IpLicense>()
                      .WithMany()
                      .HasForeignKey("ip_id")
                      .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("ip_license_regions");
                    j.HasKey("ip_id", "region_id");
                    j.HasIndex("region_id");


                });
        }
    }
}
