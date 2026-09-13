using WorldPinball.Domain.Common;

namespace WorldPinball.Domain.Events;

public sealed record GameSessionEndedDomainEvent(
    Guid SessionId,
    DateTime EndedAtUtc
) : IDomainEvent;
