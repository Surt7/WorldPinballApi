namespace WorldPinball.Domain.Entities;

public class GameSessionPlayer
{
    // 1. Session reference
    public Guid SessionId { get; set; }
    public GameSession Session { get; set; } = null!;

    // 2. Player slot on the machine (1, 2, 3 or 4)
    public int SlotNumber { get; set; }
    
    // 3. Player identification:
    // - If registered Keycloak user: UserId is set (e.g. Keycloak sub GUID)
    // - If guest / anonymous: UserId is null, and GuestInitials can be set (e.g. "TOM", "AAA")
    public Guid? UserId { get; set; }
    public UserProfile? UserProfile { get; set; }

    public string? GuestInitials { get; set; }

    // 4. Metadata
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}