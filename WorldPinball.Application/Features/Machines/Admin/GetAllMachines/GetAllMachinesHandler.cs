using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Admin.GetAllMachines.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.Machines.Admin.GetAllMachines;

public sealed class GetAllMachinesHandler(
    IMachineReadRepository readRepository,
    ILogger<GetAllMachinesHandler> logger
) : IGetAllMachinesHandler
{
    private static readonly EventId FetchingMachines = new(3100, nameof(FetchingMachines));
    private static readonly EventId NoMachinesFound = new(3101, nameof(NoMachinesFound));

    public async Task<GetAllMachinesResult> HandleAsync(
        GetAllMachinesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(FetchingMachines, "Fetching all registered machines.");
        var machines = await readRepository.GetAllMachines(cancellationToken);

        if (machines.Count == 0)
        {
            logger.LogWarning(NoMachinesFound, "No machines found in the system.");
            return new GetAllMachinesResult.NotFound();
        }

        return new GetAllMachinesResult.Success(machines);
    }
}
