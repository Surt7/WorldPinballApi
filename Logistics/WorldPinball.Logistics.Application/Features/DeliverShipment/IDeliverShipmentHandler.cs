using WorldPinball.Logistics.Application.Features.DeliverShipment.Commands;

namespace WorldPinball.Logistics.Application.Features.DeliverShipment;

public interface IDeliverShipmentHandler
{
    Task<DeliverShipmentResult> HandleAsync(
        DeliverShipmentCommand command, 
        CancellationToken ct = default);
}
