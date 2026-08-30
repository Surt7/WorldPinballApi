using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class MissionProgressConfiguration : IEntityTypeConfiguration<MissionProgress>
    {

        public void Configure(EntityTypeBuilder<MissionProgress> e)
        {


            // =========================
            // mission_progress
            // =========================

            e.ToTable("mission_progress");
            e.HasKey(x => x.MissionProgressId);

            e.Property(x => x.UserId).IsRequired();

            e.Property(x => x.Status)
             .HasConversion<string>()
             .HasMaxLength(20)
             .IsRequired();

            e.Property(x => x.CurrentValue).IsRequired();
            e.Property(x => x.UpdatedAtUtc).IsRequired();

            e.HasIndex(x => new { x.UserId, x.UpdatedAtUtc });
            e.HasIndex(x => x.MissionId);
            e.HasIndex(x => new { x.UserId, x.MissionId }).IsUnique();
            e.HasIndex(x => x.Status);

            e.HasOne(x => x.Mission)
             .WithMany()
             .HasForeignKey(x => x.MissionId)
             .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
