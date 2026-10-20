namespace WorldPinball.Logistics.Domain.Events;

public sealed record ShipmentCreatedDomainEvent(
    Guid ShipmentId,
    string OrderId,
    string DestinationAddress
    ):IDomainEvent;