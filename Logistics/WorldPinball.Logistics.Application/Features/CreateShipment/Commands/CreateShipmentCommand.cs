namespace WorldPinball.Logistics.Application.Features.CreateShipment.Commands;

public record CreateShipmentCommand(string OrderId, string DestinationAddress);