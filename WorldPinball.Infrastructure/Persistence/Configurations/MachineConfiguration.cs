using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class MachineConfiguration : IEntityTypeConfiguration<Machine>
    {
        public void Configure(EntityTypeBuilder<Machine> e)
        {
            // =========================
            // machines
            // =========================
            e.ToTable("machines");

            e.HasKey(x => x.MachineId);

            e.Property(x => x.IpId)
                .IsRequired();

            e.Property(x => x.WorldPinballSerialNumber)
                .HasMaxLength(32)
                .IsRequired();

            e.Property(x => x.Manufacturer)
                .HasMaxLength(100);

            e.Property(x => x.ManufacturerModelCode)
                .HasMaxLength(50);

            e.Property(x => x.ManufacturerModelName)
                .HasMaxLength(150);

            e.Property(x => x.ManufacturerModelYear)
                .HasMaxLength(20);

            e.Property(x => x.ManufacturerSerialNumber)
                .HasMaxLength(100);

            e.Property(x => x.ManufacturerNotes)
                .HasMaxLength(1000);
            
            e.Property(x => x.Nonce)
                    .HasMaxLength(64)
                .IsRequired();
            
            e.Property(x => x.IdempotencyKey)
                .HasMaxLength(32)
                .IsRequired();
            
            e.Property(x => x.MachineNotes)
                .HasMaxLength(1000);

            e.Property(x => x.Location)
                .HasMaxLength(200);

            e.Property(x => x.Voltage);

            e.Property(x => x.FrequencyHz);

            e.Property(x => x.Amperage)
                .HasPrecision(6, 2);

            e.Property(x => x.ManufacturedDate);

            e.Property(x => x.DisplayName)
                .HasMaxLength(150);

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            e.Property(x => x.RegisteredAtUtc)
                .IsRequired();

            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.WorldPinballSerialNumber)
                .IsUnique();

            e.HasIndex(x => x.ManufacturerSerialNumber);

            e.HasIndex(x => x.IpId);

            e.HasIndex(x => x.Status);

            e.HasOne(x => x.IpLicense)
                .WithMany(x => x.Machines)
                .HasForeignKey(x => x.IpId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
