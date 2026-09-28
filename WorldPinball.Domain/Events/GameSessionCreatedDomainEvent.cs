using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionCreatedDomainEvent(
    Guid SessionId,
    Guid MachineId,
    Guid DeviceId,
    Guid ModeId,
    DateTime CreatedAtUtc
) : IDomainEvent;