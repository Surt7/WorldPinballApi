using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;

namespace WorldPinball.Logistics.Application.Features.CreateShipment.Commands;

public sealed class CreateShipmentHandler(
    IShipmentWriteRepository repository,
    ILogger<CreateShipmentHandler> logger
    ) : ICreateShipmentHandler
{
    public static class ErrorMessages
    {
        public const string EmptyOrderId = "The order identifier cannot be empty.";
        public const string EmptyAddress = "The destination address cannot be empty.";
    }

    private static readonly EventId ShipmentCreated = new(4010, nameof(ShipmentCreated));
    private static readonly EventId ShipmentAlreadyExistsEvent = new(4011, nameof(ShipmentAlreadyExistsEvent));
    private static readonly EventId InvalidShipmentData = new(4012, nameof(InvalidShipmentData));

    public async Task<CreateShipmentResult> HandleAsync(CreateShipmentCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.OrderId))
        {
            logger.LogWarning(InvalidShipmentData, "Create shipment failed: OrderId is empty");
            return new CreateShipmentResult.InvalidOrderData(ErrorMessages.EmptyOrderId);
        }

        if (string.IsNullOrWhiteSpace(command.DestinationAddress))
        {
            logger.LogWarning(InvalidShipmentData, "Create shipment failed: DestinationAddress is empty");
            return new CreateShipmentResult.InvalidOrderData(ErrorMessages.EmptyAddress);
        }

        bool exists = await repository.ExistsByOrderIdAsync(command.OrderId, ct);
        if (exists)
        {
            logger.LogWarning(ShipmentAlreadyExistsEvent, "Shipment already exists for order {OrderId}", command.OrderId);
            return new CreateShipmentResult.ShipmentAlreadyExists(command.OrderId);
        }

        var shipment = Shipment.Create(command.OrderId, command.DestinationAddress);
        await repository.AddAsync(shipment, ct);

        logger.LogInformation(ShipmentCreated, "Shipment {ShipmentId} created for order {OrderId}", shipment.ShipmentId, shipment.OrderId);

        return new CreateShipmentResult.Success(shipment.ShipmentId);
    }
}