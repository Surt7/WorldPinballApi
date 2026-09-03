
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
    {

        public void Configure(EntityTypeBuilder<Mission> e)
        {
            
            // =========================
            // missions
            // =========================

            e.ToTable("missions");
            e.HasKey(x => x.MissionId);

            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Title).IsRequired();

            e.Property(x => x.MissionType)
             .HasConversion<string>()
             .HasMaxLength(30)
             .IsRequired();

            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.IsActive);
            e.HasIndex(x => x.IpId);
            e.HasIndex(x => x.ModeId);

            e.HasOne(x => x.IpLicense)
             .WithMany()
             .HasForeignKey(x => x.IpId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.GameMode)
      .WithMany()
      .HasForeignKey(x => x.ModeId)
      .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
