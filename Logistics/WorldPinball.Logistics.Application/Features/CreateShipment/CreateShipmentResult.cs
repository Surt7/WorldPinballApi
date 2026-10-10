namespace WorldPinball.Logistics.Application.Features.CreateShipment;

public abstract record CreateShipmentResult
{
    private CreateShipmentResult() { }

    public sealed record Success(Guid ShipmentId) : CreateShipmentResult;
    public sealed record ShipmentAlreadyExists(string OrderId) : CreateShipmentResult;
    public sealed record InvalidOrderData(string Reason) : CreateShipmentResult;
}