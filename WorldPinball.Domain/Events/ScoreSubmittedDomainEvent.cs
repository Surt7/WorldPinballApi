using WorldPinball.Domain.Common;
namespace WorldPinball.Domain.Events;
public sealed record ScoreSubmittedDomainEvent(
    Guid ScoreEntryId,
    Guid SessionId,
    Guid MachineId,
    Guid IpId,
    Guid ModeId,
    int SlotNumber,
    string? UserId,
    long Score,
    DateTime AchievedAtUtc
) : IDomainEvent;