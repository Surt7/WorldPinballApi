namespace WorldPinball.Logistics.Application.Features.GetShipment.Dtos;

public sealed record ShipmentDto(
    Guid ShipmentId,
    string OrderId,
    string DestinationAddress,
    string Status,
    Guid? MachineId,
    string? TrackingNumber,
    string? CancellationReason);