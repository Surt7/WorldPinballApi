using WorldPinball.Application.Features.Machines.Admin.Provision.Dtos;

namespace WorldPinball.Application.Features.Machines.Admin.GetAllMachines;

public abstract record GetAllMachinesResult
{
    private GetAllMachinesResult() { }

    public sealed record Success(IReadOnlyList<MachineRowDto> Machines) : GetAllMachinesResult;
    public sealed record NotFound : GetAllMachinesResult;
}
