using WorldPinball.Domain.Common;
using WorldPinball.Domain.Events;

namespace WorldPinball.Domain.Entities;

public class ScoreEntry:Entity
{
    private static class ErrorMessages
    {
        public const string SessionId = "SessionId cannot be empty.";
        public const string DeviceId = "DeviceId cannot be empty.";
        public const string MachineId = "MachineId cannot be empty.";
        public const string IpId = "IpId cannot be empty.";
        public const string ModeId = "ModeId cannot be empty.";
        public const string InvalidSlotNumber = "Slot number must be between 1 and 4.";
        public const string InvalidScore = "Score cannot be negative.";
        public const string InvalidReason = "A validation reason is required.";
    }

    private ScoreEntry() { }

    public Guid ScoreEntryId { get; private set; }
    public int SlotNumber { get; private set; }

    public Guid SessionId { get; private set; }
    public GameSession Session { get; private set; } = null!;

    public string? UserId { get; private set; }

    public Guid DeviceId { get; private set; }
    public Device Device { get; private set; } = null!;

    public Guid MachineId { get; private set; }
    public Machine Machine { get; private set; } = null!;

    public Guid IpId { get; private set; }
    public IpLicense IpLicense { get; private set; } = null!;

    public Guid ModeId { get; private set; }
    public GameMode Mode { get; private set; } = null!;

    public long Score { get; private set; }
    public DateTime AchievedAtUtc { get; private set; }

    public bool IsValidated { get; private set; }
    public string? ValidationReason { get; private set; }

    public ScoreEntry(
        Guid sessionId,
        int slotNumber,
        Guid deviceId,
        Guid machineId,
        Guid ipId,
        Guid modeId,
        long score,
        DateTime achievedAtUtc,
        string? userId = null)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException(ErrorMessages.SessionId, nameof(sessionId));
        if (deviceId == Guid.Empty) throw new ArgumentException(ErrorMessages.DeviceId, nameof(deviceId));
        if (machineId == Guid.Empty) throw new ArgumentException(ErrorMessages.MachineId, nameof(machineId));
        if (ipId == Guid.Empty) throw new ArgumentException(ErrorMessages.IpId, nameof(ipId));
        if (modeId == Guid.Empty) throw new ArgumentException(ErrorMessages.ModeId, nameof(modeId));
        if (slotNumber is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(slotNumber), ErrorMessages.InvalidSlotNumber);
        if (score < 0) throw new ArgumentOutOfRangeException(nameof(score), ErrorMessages.InvalidScore);

        ScoreEntryId = Guid.NewGuid();
        SessionId = sessionId;
        SlotNumber = slotNumber;
        DeviceId = deviceId;
        MachineId = machineId;
        IpId = ipId;
        ModeId = modeId;
        Score = score;
        AchievedAtUtc = achievedAtUtc;
        UserId = userId;
        IsValidated = true;
        
        AddDomainEvent(new ScoreSubmittedDomainEvent(
            ScoreEntryId,
            SessionId,
            MachineId,
            ModeId,
            SlotNumber,
            UserId,
            Score,
            AchievedAtUtc));
        
    }

    public void Invalidate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(ErrorMessages.InvalidReason, nameof(reason));

        IsValidated = false;
        ValidationReason = reason;
    }
}
