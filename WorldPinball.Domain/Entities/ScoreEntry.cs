namespace WorldPinball.Domain.Entities;

public class ScoreEntry
{
    public Guid ScoreEntryId { get; set; }

    public Guid SessionId { get; set; }
    public GameSession Session { get; set; } = null!;

    public string? UserId { get; set; } = default!;

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public Guid IpId { get; set; }
    public IpLicense IpLicense { get; set; } = null!;

    public Guid ModeId { get; set; }
    public GameMode Mode { get; set; } = null!;

    public int Score { get; set; }
    public DateTime AchievedAtUtc { get; set; }

    public bool IsValidated { get; set; } = true;
    public string? ValidationReason { get; set; }
}