namespace WorldPinball.Application.Features.Leaderboard.Public.Queries;

public sealed record GetLeaderboardQuery(
    string? IpSlug,
    string? ModeCode,
    int Limit = 50
);
