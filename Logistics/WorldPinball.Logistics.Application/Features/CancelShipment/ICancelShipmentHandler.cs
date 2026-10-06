using WorldPinball.Logistics.Application.Features.CancelShipment.Commands;

namespace WorldPinball.Logistics.Application.Features.CancelShipment;

public interface ICancelShipmentHandler
{
    Task<CancelShipmentResult> HandleAsync(
        CancelShipmentCommand command, 
        CancellationToken ct = default);
}