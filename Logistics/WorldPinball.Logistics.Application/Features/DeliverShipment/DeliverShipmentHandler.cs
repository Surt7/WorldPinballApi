using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Features.DeliverShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Application.Features.DeliverShipment;

public sealed class DeliverShipmentHandler(
    IShipmentWriteRepository repository,
    ILogger<DeliverShipmentHandler> logger
) : IDeliverShipmentHandler
{
    private static readonly EventId ShipmentDelivered = new(4040, nameof(ShipmentDelivered));
    private static readonly EventId ShipmentNotFoundEvent = new(4041, nameof(ShipmentNotFoundEvent));
    private static readonly EventId CannotDeliverEvent = new(4042, nameof(CannotDeliverEvent));

    public async Task<DeliverShipmentResult> HandleAsync(
        DeliverShipmentCommand command, 
        CancellationToken ct = default)
    {
        var shipment = await repository.GetByIdAsync(command.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning(ShipmentNotFoundEvent, "Shipment {ShipmentId} not found for delivery", command.ShipmentId);
            return new DeliverShipmentResult.ShipmentNotFound(command.ShipmentId);
        }

        try
        {
            shipment.Deliver();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(CannotDeliverEvent, ex, "Cannot deliver shipment {ShipmentId}: {Reason}", command.ShipmentId, ex.Message);
            return new DeliverShipmentResult.CannotDeliver(ex.Message);
        }

        await repository.UpdateAsync(shipment, ct);

        logger.LogInformation(ShipmentDelivered, "Shipment {ShipmentId} successfully delivered", command.ShipmentId);

        return new DeliverShipmentResult.Success();
    }
}
