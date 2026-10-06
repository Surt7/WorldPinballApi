using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Features.AllocateMachine.Commands;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Application.Features.AllocateMachine;

public sealed class AllocateMachineHandler(
    IShipmentWriteRepository repository,
    ILogger<AllocateMachineHandler> logger) : IAllocateMachineHandler
{
    private static readonly EventId MachineAllocated = new(4020, nameof(MachineAllocated));
    private static readonly EventId ShipmentNotFoundEvent = new(4021, nameof(ShipmentNotFoundEvent));
    private static readonly EventId AllocationFailedEvent = new(4022, nameof(AllocationFailedEvent));

    public async Task<AllocateMachineResult> HandleAsync(AllocateMachineCommand command, CancellationToken ct = default)
    {
        if (command.MachineId == Guid.Empty)
        {
            logger.LogWarning(AllocationFailedEvent, "Allocation failed: MachineId is empty for shipment {ShipmentId}", command.ShipmentId);
            return new AllocateMachineResult.InvalidMachineId();
        }

        var shipment = await repository.GetByIdAsync(command.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning(ShipmentNotFoundEvent, "Shipment {ShipmentId} not found", command.ShipmentId);
            return new AllocateMachineResult.ShipmentNotFound(command.ShipmentId);
        }

        try
        {
            shipment.AllocateMachine(command.MachineId);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(AllocationFailedEvent, ex, "Cannot allocate machine to shipment {ShipmentId}: {Reason}", command.ShipmentId, ex.Message);
            return new AllocateMachineResult.CannotAllocate(ex.Message);
        }

        await repository.UpdateAsync(shipment, ct);

        logger.LogInformation(MachineAllocated, "Machine {MachineId} allocated to shipment {ShipmentId}",
            command.MachineId, command.ShipmentId);

        return new AllocateMachineResult.Success();
    }
}