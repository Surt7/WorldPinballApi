namespace WorldPinball.Logistics.Application.Features.AllocateMachine.Commands;

public record AllocateMachineCommand(Guid ShipmentId, Guid MachineId);