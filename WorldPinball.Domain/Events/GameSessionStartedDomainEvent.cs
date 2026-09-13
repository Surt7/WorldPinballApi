using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionStartedDomainEvent(
    Guid SessionId,
    Guid MachineId,
    Guid DeviceId,
    Guid ModeId,
    DateTime StartedAtUtc
) : IDomainEvent;
