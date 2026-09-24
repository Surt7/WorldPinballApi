using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;

namespace WorldPinball.Application.Features.Leaderboard.Admin.Dashboard;

public abstract record GetMachineDashboardResult
{
    private GetMachineDashboardResult() {}
    
    public sealed record Success(MachineDashboardDto Stats) : GetMachineDashboardResult;
    public sealed record NotFound(Guid MachineId) : GetMachineDashboardResult;
};