namespace WorldPinball.Logistics.Application.Features.CancelShipment;
public abstract record CancelShipmentResult
{
    private CancelShipmentResult() { }
    
    public sealed record Success : CancelShipmentResult;
    
    public sealed record ShipmentNotFound(Guid ShipmentId) : CancelShipmentResult;
    
    public sealed record CannotCancel(string Reason) : CancelShipmentResult;
    
    public sealed record InvalidCancellationReason : CancelShipmentResult;
}