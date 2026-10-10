using WorldPinball.Logistics.Application.Features.DispatchShipment.Commands;

namespace WorldPinball.Logistics.Application.Features.DispatchShipment;

public interface IDispatchShipmentHandler
{
    Task<DispatchShipmentResult> HandleAsync(
        DispatchShipmentCommand command, 
        CancellationToken ct = default);
}