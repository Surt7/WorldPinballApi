using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

public sealed class EnrollmentTokenConfiguration : IEntityTypeConfiguration<EnrollmentToken>
{
    public void Configure(EntityTypeBuilder<EnrollmentToken> b)
    {
        b.ToTable("enrollment_tokens");
        b.HasKey(x => x.TokenId);

        b.Property(x => x.TokenId).HasColumnName("token_id");
        b.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(256);
        b.Property(x => x.TokenType).HasColumnName("token_type").HasConversion<string>().HasMaxLength(20).IsRequired();

        b.Property(x => x.MachineId).HasColumnName("machine_id");
        b.Property(x => x.DeviceId).HasColumnName("device_id");
        b.Property(x => x.IssuedByUserId).HasColumnName("issued_by_user_id").IsRequired().HasMaxLength(450);
        b.Property(x => x.ConsumedByDeviceId).HasColumnName("consumed_by_device_id");

        b.Property(x => x.IssuedAtUtc).HasColumnName("issued_at").IsRequired();
        b.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at").IsRequired();
        b.Property(x => x.ConsumedAtUtc).HasColumnName("consumed_at");
        b.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        
        b.Property(x => x.MaxUses).HasColumnName("max_uses").IsRequired();
        b.Property(x => x.UsesCount).HasColumnName("uses_count").IsRequired();
        b.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);

        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.TokenType, x.ExpiresAtUtc });
        b.HasIndex(x => x.MachineId);
        b.HasIndex(x => x.DeviceId);
        b.HasIndex(x => x.IssuedByUserId);
        b.HasIndex(x => x.ConsumedByDeviceId);

        // ---- relations (verrouillées) ----

        b.HasOne(x => x.Machine)
            .WithMany(x => x.EnrollmentTokens)
            .HasForeignKey(x => x.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Device)
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ConsumedByDevice)
            .WithMany(x => x.ConsumedTokens)
            .HasForeignKey(x => x.ConsumedByDeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
