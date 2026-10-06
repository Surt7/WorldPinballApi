namespace WorldPinball.Logistics.Application.Features.AllocateMachine;

public abstract record AllocateMachineResult
{
    private AllocateMachineResult() { }

    public sealed record Success : AllocateMachineResult;
    public sealed record ShipmentNotFound(Guid ShipmentId) : AllocateMachineResult;
    public sealed record CannotAllocate(string Reason) : AllocateMachineResult;
    public sealed record InvalidMachineId : AllocateMachineResult;
}