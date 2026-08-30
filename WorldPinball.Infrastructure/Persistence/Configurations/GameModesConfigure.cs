using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations
{
    public sealed class GameModesConfigure : IEntityTypeConfiguration<GameMode>
    {
        public void Configure(EntityTypeBuilder<GameMode> e)
        {
            // =========================
            // game_modes
            // =========================

            e.ToTable("game_modes");
            e.HasKey(x => x.ModeId);

            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description);

            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc);

            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.IsActive);

        }
    }
}
