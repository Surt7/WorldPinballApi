namespace WorldPinball.Logistics.Application.Features.DispatchShipment.Commands;

public sealed record DispatchShipmentCommand(
    Guid ShipmentId,
    string TrackingNumber
);