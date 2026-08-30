using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class GameSession
{
    public Guid SessionId { get; set; }

    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? UserId { get; set; }

    public Guid ModeId { get; set; }
    public GameMode Mode { get; set; } = null!;

    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    public GameSessionState State { get; set; } = GameSessionState.Started;
    public string Nonce { get; set; } = null!;
    public string? ClientVersion { get; set; }

    public ScoreEntry? ScoreEntry { get; set; }
}