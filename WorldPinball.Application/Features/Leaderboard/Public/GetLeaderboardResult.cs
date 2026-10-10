using WorldPinball.Application.Features.Leaderboard.Public.Dtos;

namespace WorldPinball.Application.Features.Leaderboard.Public;

public abstract record GetLeaderboardResult
{
    private GetLeaderboardResult() { }

    public sealed record Success(IReadOnlyList<LeaderboardRowDto> Rows) : GetLeaderboardResult;
    public sealed record InvalidParameters(
        string Message) : GetLeaderboardResult;
};