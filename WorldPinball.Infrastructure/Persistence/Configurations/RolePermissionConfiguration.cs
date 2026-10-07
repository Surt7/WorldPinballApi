using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Infrastructure.Persistence.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => new { rp.Role, rp.Permission });

        builder.Property(rp => rp.Role)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rp => rp.Permission)
            .HasMaxLength(100)
            .IsRequired();
    }
}
