using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record BallScoreRecordedDomainEvent(
    Guid SessionId,
    Guid MachineId,
    int SlotNumber,
    int BallNumber,
    long Score,
    DateTime RecordedAtUtc
) : IDomainEvent;