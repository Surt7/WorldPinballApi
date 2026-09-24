using Microsoft.Extensions.Logging;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Queries;


public sealed class GetMachineDashboardHandler(
    IMachineDashboardReadRepository readRepository,
    ILogger<GetMachineDashboardHandler> logger
) : IGetMachineDashboardHandler
{
    private static readonly EventId StatNotFound = new EventId(2600, nameof(StatNotFound));
    private static readonly EventId InvalidMachineId = new EventId(2601, nameof(InvalidMachineId));
    
    public async Task<GetMachineDashboardResult> HandleAsync(GetMachineDashboardQuery query, CancellationToken cancellationToken = default)
    {

        if (query.MachineId == Guid.Empty)
        {
            logger.LogWarning(InvalidMachineId, "Machine ID cannot be empty.");
            return new GetMachineDashboardResult.NotFound(query.MachineId);
        }
        
        var stat = await readRepository.GetMachineStatsAsync(query.MachineId, cancellationToken);
        
        if (stat is null)
        {
            logger.LogWarning(
                StatNotFound,
                "Stat on machine: {machineId}  could not be found.",
                query.MachineId
            );

            return new GetMachineDashboardResult.NotFound(query.MachineId);
        }
        return new GetMachineDashboardResult.Success(stat);
        
    }
}