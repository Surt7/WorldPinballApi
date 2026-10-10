using WorldPinball.Application.Features.Leaderboard.Public.Queries;

namespace WorldPinball.Application.Features.Leaderboard.Public;

public interface IGetLeaderboardHandler
{
    Task<GetLeaderboardResult> HandleAsync(
        GetLeaderboardQuery query,
        CancellationToken ct = default);
}