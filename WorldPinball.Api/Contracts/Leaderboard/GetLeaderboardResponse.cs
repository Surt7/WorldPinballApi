namespace WorldPinball.Api.Contracts.Leaderboard;

using WorldPinball.Application.Features.Leaderboard.Public.Dtos;

public sealed record GetLeaderboardResponse(
    IReadOnlyList<LeaderboardRowDto> Rows
);