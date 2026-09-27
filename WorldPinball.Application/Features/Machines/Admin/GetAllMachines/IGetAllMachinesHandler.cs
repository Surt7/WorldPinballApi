using WorldPinball.Application.Features.Machines.Admin.GetAllMachines.Queries;

namespace WorldPinball.Application.Features.Machines.Admin.GetAllMachines;

public interface IGetAllMachinesHandler
{
    Task<GetAllMachinesResult> HandleAsync(
        GetAllMachinesQuery query,
        CancellationToken cancellationToken = default);
}
