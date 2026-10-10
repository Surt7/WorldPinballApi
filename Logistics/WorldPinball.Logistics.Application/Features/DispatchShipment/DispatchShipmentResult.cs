namespace WorldPinball.Logistics.Application.Features.DispatchShipment;

public abstract record DispatchShipmentResult
{
    private DispatchShipmentResult() { }
    
    public sealed record Success : DispatchShipmentResult;
    
    public sealed record ShipmentNotFound(Guid ShipmentId) : DispatchShipmentResult;
    
    public sealed record CannotDispatch(string Reason) : DispatchShipmentResult;
    
    public sealed record InvalidTrackingNumber : DispatchShipmentResult;
}