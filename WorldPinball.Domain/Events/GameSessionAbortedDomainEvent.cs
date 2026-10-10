using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionAbortedDomainEvent(
    Guid SessionId,
    Guid MachineId,
    DateTime AbortedAtUtc
) : IDomainEvent;