using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;
using WorldPinball.Application.Features.Leaderboard.Public.Queries.GetLeaderboard;

namespace WorldPinball.Application.Features.Leaderboard.Public.Handlers;

public sealed class GetLeaderboardHandler(ILeaderboardRepository repository)
{
    public Task<IReadOnlyList<LeaderboardRowDto>> HandleAsync(
        GetLeaderboardQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(query.Limit, 1, 200);

        return repository.GetLeaderboardAsync(
            query.IpSlug,
            query.ModeCode,
            limit,
            cancellationToken);
    }
}