namespace WorldPinball.Logistics.Application.Features.CancelShipment.Commands;

public sealed record CancelShipmentCommand(
    Guid ShipmentId,
    string Reason
);