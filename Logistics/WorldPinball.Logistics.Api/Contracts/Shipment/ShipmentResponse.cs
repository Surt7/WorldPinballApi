namespace WorldPinball.Logistics.Api.Contracts.Shipment;


public sealed record ShipmentResponse(
    Guid ShipmentId,
    string OrderId,
    string DestinationAddress,
    string Status,
    Guid? MachineId,
    string? TrackingNumber,
    string? CancellationReason);