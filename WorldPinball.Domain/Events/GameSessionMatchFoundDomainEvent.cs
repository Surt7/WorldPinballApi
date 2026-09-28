using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionMatchFoundDomainEvent(
    Guid SessionId,
    Guid MachineId,
    int PlayerCount,
    DateTime MatchFoundAtUtc
) : IDomainEvent;