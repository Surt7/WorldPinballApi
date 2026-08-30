using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;
namespace WorldPinball.Infrastructure.Persistence.Configurations;

public sealed class CertificateRevocationConfiguration : IEntityTypeConfiguration<CertificateRevocation>
{
    public void Configure(EntityTypeBuilder<CertificateRevocation> b)
    {
        b.ToTable("certificate_revocations");
        b.HasKey(x => x.RevocationId);

        b.Property(x => x.RevokedAtUtc).IsRequired();
        b.Property(x => x.Reason).IsRequired();
        b.Property(x => x.RevokedByUserId).IsRequired();

        b.HasIndex(x => x.RevokedAtUtc);
        b.HasIndex(x => x.RevokedByUserId);

        // 1:1 (unique FK)
        b.HasOne(x => x.DeviceCertificate)
            .WithOne(x => x.Revocation)
            .HasForeignKey<CertificateRevocation>(x => x.DeviceCertId);

    }
}
