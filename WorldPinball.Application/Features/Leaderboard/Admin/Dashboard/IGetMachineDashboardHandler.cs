using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Queries;

namespace WorldPinball.Application.Features.Leaderboard.Admin.Dashboard;

public interface IGetMachineDashboardHandler
{
    Task<GetMachineDashboardResult> HandleAsync(
        GetMachineDashboardQuery query,
        CancellationToken cancellationToken = default
    );
}