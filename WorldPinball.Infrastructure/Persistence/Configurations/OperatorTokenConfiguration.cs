using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public sealed class OperatorTokenConfiguration : IEntityTypeConfiguration<OperatorToken>
{
    public void Configure(EntityTypeBuilder<OperatorToken> b)
    {
        b.ToTable("operator_tokens");
        b.HasKey(x => x.TokenId);

        b.Property(x => x.TokenId).HasColumnName("token_id");
        b.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        b.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(64);
        b.Property(x => x.Prefix).HasColumnName("prefix").IsRequired().HasMaxLength(32);
        b.Property(x => x.Scopes).HasColumnName("scopes").IsRequired().HasMaxLength(500);
        b.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        b.Property(x => x.IssuedByUserId).HasColumnName("issued_by_user_id").HasMaxLength(450);

        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at").IsRequired();
        b.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at");
        b.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at");

        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.IsActive);
    }
}
