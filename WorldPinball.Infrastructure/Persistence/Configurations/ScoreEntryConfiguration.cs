using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;


namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class ScoreEntryConfiguration : IEntityTypeConfiguration<ScoreEntry>
    {

        public void Configure(EntityTypeBuilder<ScoreEntry> e)
        {
            // =========================
            // score_entries
            // =========================

            e.ToTable("score_entries");
            e.HasKey(x => x.ScoreEntryId);
            e.Property(x => x.SlotNumber).IsRequired();
            e.Property(x => x.SessionId).IsRequired();
            e.Property(x => x.DeviceId).IsRequired();
            e.Property(x => x.MachineId).IsRequired();
            e.Property(x => x.IpId).IsRequired();
            e.Property(x => x.ModeId).IsRequired();

            e.Property(x => x.Score).IsRequired();
            e.Property(x => x.AchievedAtUtc).IsRequired();

            e.Property(x => x.IsValidated).IsRequired();
            e.Property(x => x.ValidationReason);

   
            e.HasIndex(x => x.SessionId);

            e.HasIndex(x => new { x.IpId, x.ModeId, x.Score, x.AchievedAtUtc });
            e.HasIndex(x => new { x.DeviceId, x.AchievedAtUtc });
            e.HasIndex(x => new { x.MachineId, x.AchievedAtUtc });
            e.HasIndex(x => new { x.UserId, x.AchievedAtUtc });

            e.HasOne(x => x.Session)
             .WithOne(x => x.ScoreEntry)
             .HasForeignKey<ScoreEntry>(x => x.SessionId)
             .OnDelete(DeleteBehavior.Cascade);

         
            e.HasOne(x => x.Device)
             .WithMany()
             .HasForeignKey(x => x.DeviceId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Machine)
             .WithMany()
             .HasForeignKey(x => x.MachineId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.IpLicense)
             .WithMany()
             .HasForeignKey(x => x.IpId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Mode)
             .WithMany()
             .HasForeignKey(x => x.ModeId)
             .OnDelete(DeleteBehavior.Restrict);


            e.HasIndex(x => new { x.SessionId, x.SlotNumber }).IsUnique();

            e.HasOne(x => x.Session)
                .WithMany(x => x.ScoreEntries)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
