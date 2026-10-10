
namespace WorldPinball.Logistics.Application.Features.GetShipment;

public abstract record GetShipmentByIdResult
{
    private GetShipmentByIdResult() { }

    public sealed record Success(Dtos.ShipmentDto Shipment) : GetShipmentByIdResult;
    public sealed record NotFound(Guid ShipmentId) : GetShipmentByIdResult;
}