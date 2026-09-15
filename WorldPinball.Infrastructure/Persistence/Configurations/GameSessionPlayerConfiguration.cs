using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Configurations;

public class GameSessionPlayerConfiguration:IEntityTypeConfiguration<GameSessionPlayer>
{
    public void Configure(EntityTypeBuilder<GameSessionPlayer> e)
    {
     
        // =========================
        // game_sessions_player 
        // =========================

        e.ToTable("game_session_players");
        
        // Composite Primary Key (SessionId, SlotNumber)
        e.HasKey(x => new { x.SessionId, x.SlotNumber });

        e.Property(x => x.SessionId).IsRequired();
        e.Property(x => x.SlotNumber).IsRequired();

        // GuestInitials is optional, max 4 characters (e.g. "AAA", "TOM")
        e.Property(x => x.GuestInitials).HasMaxLength(4);

        e.Property(x => x.JoinedAtUtc).IsRequired();

        // Index to quickly search session history for a specific user
        e.HasIndex(x => new { x.UserId, x.JoinedAtUtc });

        // Relationship with GameSession (One Session -> Many Players)
        e.HasOne(x => x.Session)
            .WithMany(x => x.Players)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}