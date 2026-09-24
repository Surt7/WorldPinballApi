using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IMachineDashboardReadRepository
{
    Task<MachineDashboardDto?> GetMachineStatsAsync(Guid machineId, CancellationToken ct = default);
}
