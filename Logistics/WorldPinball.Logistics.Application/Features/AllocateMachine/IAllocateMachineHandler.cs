using WorldPinball.Logistics.Application.Features.AllocateMachine.Commands;

namespace WorldPinball.Logistics.Application.Features.AllocateMachine;

public interface IAllocateMachineHandler
{
    Task<AllocateMachineResult> HandleAsync(AllocateMachineCommand command, CancellationToken ct = default);
}