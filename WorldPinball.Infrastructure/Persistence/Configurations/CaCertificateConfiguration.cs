using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

internal sealed class CaCertificateConfiguration : IEntityTypeConfiguration<CaCertificate>
{
    public void Configure(EntityTypeBuilder<CaCertificate> b)
    {
        b.ToTable("ca_certificates");
        b.HasKey(x => x.CaId);

        b.Property(x => x.CaType).IsRequired().HasMaxLength(20); // root | intermediate
        b.Property(x => x.SubjectDn).IsRequired();
        b.Property(x => x.IssuerDn).IsRequired();
        b.Property(x => x.SerialNumber).IsRequired();
        b.Property(x => x.ThumbprintSha256).IsRequired();
        b.Property(x => x.CertPem).IsRequired();

        b.Property(x => x.PrivateKeyRef);

        b.Property(x => x.NotBeforeUtc).IsRequired();
        b.Property(x => x.NotAfterUtc).IsRequired();

        b.Property(x => x.IsTrusted).IsRequired();
        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        b.HasIndex(x => x.ThumbprintSha256).IsUnique();
        b.HasIndex(x => x.NotAfterUtc);
        b.HasIndex(x => new { x.CaType, x.IsActive });
    }
}