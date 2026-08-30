using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

public sealed class DeviceCertificateConfiguration : IEntityTypeConfiguration<DeviceCertificate>
{
    public void Configure(EntityTypeBuilder<DeviceCertificate> b)
    {
        b.ToTable("device_certificates");
        b.HasKey(x => x.DeviceCertId);

        b.Property(x => x.CertSerialNumber).IsRequired();
        b.Property(x => x.CertThumbprintSha256).IsRequired();
        b.HasIndex(x => x.CertThumbprintSha256).IsUnique();

        b.Property(x => x.SubjectDn).IsRequired();
        b.Property(x => x.IssuerDn).IsRequired();

        b.Property(x => x.CertPem).IsRequired();

        b.Property(x => x.NotBeforeUtc).IsRequired();
        b.Property(x => x.NotAfterUtc).IsRequired();

        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.IssuedAtUtc).IsRequired();

        b.HasIndex(x => new { x.DeviceId, x.Status });
        b.HasIndex(x => new { x.DeviceId, x.NotAfterUtc });
        b.HasIndex(x => x.CaId);

        b.HasOne(x => x.Device)
            .WithMany(x => x.Certificates)
            .HasForeignKey(x => x.DeviceId);

        b.HasOne(x => x.CaCertificate)
            .WithMany()
            .HasForeignKey(x => x.CaId);

        // Rotation self-ref: superseded_by_device_cert_id -> device_cert_id
        b.HasOne(x => x.SupersededBy)
            .WithMany()
            .HasForeignKey(x => x.SupersededByDeviceCertId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}