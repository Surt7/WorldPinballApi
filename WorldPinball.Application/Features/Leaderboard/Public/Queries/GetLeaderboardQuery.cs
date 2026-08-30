namespace WorldPinball.Application.Features.Leaderboard.Public.Queries.GetLeaderboard;

public sealed record GetLeaderboardQuery(
    string? IpSlug,
    string? ModeCode,
    int Limit = 50
);