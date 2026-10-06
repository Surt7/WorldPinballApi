namespace WorldPinball.Logistics.Application.Features.DeliverShipment;

public abstract record DeliverShipmentResult
{
    private DeliverShipmentResult() { }

    public sealed record Success : DeliverShipmentResult;
    public sealed record ShipmentNotFound(Guid ShipmentId) : DeliverShipmentResult;
    public sealed record CannotDeliver(string Reason) : DeliverShipmentResult;
}
