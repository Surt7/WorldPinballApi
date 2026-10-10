using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionLaunchedDomainEvent(
    Guid SessionId,
    Guid MachineId,
    DateTime LaunchedAtUtc
) : IDomainEvent;
