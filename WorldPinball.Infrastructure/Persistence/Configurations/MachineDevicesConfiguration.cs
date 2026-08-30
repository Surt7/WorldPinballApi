using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class MachineDevicesConfiguration : IEntityTypeConfiguration<MachineDevice>
    {
        public void Configure(EntityTypeBuilder<MachineDevice> e)
        {
            // =========================
            // machine_devices (pivot)
            // =========================

            e.ToTable("machine_devices");

            e.HasKey(x => new { x.MachineId, x.DeviceId });

            e.Property(x => x.AttachedAtUtc).IsRequired();
            e.Property(x => x.DetachedAtUtc);
            e.Property(x => x.IsActive).IsRequired();

            e.HasIndex(x => x.DeviceId);
            e.HasIndex(x => new { x.MachineId, x.IsActive });

            e.HasOne(x => x.Machine)
             .WithMany(m => m.MachineDevices)
             .HasForeignKey(x => x.MachineId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Device)
             .WithMany(d => d.MachineDevices)
             .HasForeignKey(x => x.DeviceId)
             .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
