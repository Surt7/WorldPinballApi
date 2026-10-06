using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Features.DispatchShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Application.Features.DispatchShipment;

public sealed class DispatchShipmentHandler(
    IShipmentWriteRepository repository,
    ILogger<DispatchShipmentHandler> logger
) : IDispatchShipmentHandler
{
    private static readonly EventId ShipmentDispatched = new(4030, nameof(ShipmentDispatched));
    private static readonly EventId ShipmentNotFoundEvent = new(4031, nameof(ShipmentNotFoundEvent));
    private static readonly EventId InvalidTrackingNumberEvent = new(4032, nameof(InvalidTrackingNumberEvent));
    private static readonly EventId CannotDispatchEvent = new(4033, nameof(CannotDispatchEvent));

    public async Task<DispatchShipmentResult> HandleAsync(
        DispatchShipmentCommand command, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.TrackingNumber))
        {
            logger.LogWarning(InvalidTrackingNumberEvent, "Dispatch failed: tracking number is empty for shipment {ShipmentId}", command.ShipmentId);
            return new DispatchShipmentResult.InvalidTrackingNumber();
        }

        var shipment = await repository.GetByIdAsync(command.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning(ShipmentNotFoundEvent, "Shipment {ShipmentId} not found for dispatch", command.ShipmentId);
            return new DispatchShipmentResult.ShipmentNotFound(command.ShipmentId);
        }

        try
        {
            shipment.Dispatch(command.TrackingNumber);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(CannotDispatchEvent, ex, "Cannot dispatch shipment {ShipmentId}: {Reason}", command.ShipmentId, ex.Message);
            return new DispatchShipmentResult.CannotDispatch(ex.Message);
        }

        await repository.UpdateAsync(shipment, ct);

        logger.LogInformation(ShipmentDispatched, 
            "Shipment {ShipmentId} dispatched with tracking {TrackingNumber}", 
            command.ShipmentId, command.TrackingNumber);

        return new DispatchShipmentResult.Success();
    }
}