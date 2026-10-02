namespace WorldPinball.Logistics.Domain.Events;

public sealed record ShipmentCancelledDomainEvent(
    Guid ShipmentId,
    string OrderId,
    string Reason
) : IDomainEvent;