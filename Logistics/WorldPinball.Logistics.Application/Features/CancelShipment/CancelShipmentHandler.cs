using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Features.CancelShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Application.Features.CancelShipment;

public sealed class CancelShipmentHandler(
    IShipmentWriteRepository repository,
    ILogger<CancelShipmentHandler> logger
) : ICancelShipmentHandler
{
    private static readonly EventId ShipmentCancelled = new(4050, nameof(ShipmentCancelled));
    private static readonly EventId ShipmentNotFoundEvent = new(4051, nameof(ShipmentNotFoundEvent));
    private static readonly EventId InvalidReasonEvent = new(4052, nameof(InvalidReasonEvent));
    private static readonly EventId CannotCancelEvent = new(4053, nameof(CannotCancelEvent));

    public async Task<CancelShipmentResult> HandleAsync(
        CancelShipmentCommand command,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            logger.LogWarning(InvalidReasonEvent, 
                "Shipment {ShipmentId} has invalid cancellation reason", command.ShipmentId);
            return new CancelShipmentResult.InvalidCancellationReason();
        }

        var shipment = await repository.GetByIdAsync(command.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning(ShipmentNotFoundEvent, 
                "Shipment {ShipmentId} not found for cancellation", command.ShipmentId);
            return new CancelShipmentResult.ShipmentNotFound(command.ShipmentId);
        }

        try
        {
            shipment.Cancel(command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(CannotCancelEvent, ex, 
                "Cannot cancel shipment {ShipmentId}: {Reason}", command.ShipmentId, ex.Message);
            return new CancelShipmentResult.CannotCancel(ex.Message);
        }

        await repository.UpdateAsync(shipment, ct);

        logger.LogInformation(ShipmentCancelled, 
            "Shipment {ShipmentId} cancelled. Reason: {Reason}", 
            command.ShipmentId, command.Reason);

        return new CancelShipmentResult.Success();
    }
}