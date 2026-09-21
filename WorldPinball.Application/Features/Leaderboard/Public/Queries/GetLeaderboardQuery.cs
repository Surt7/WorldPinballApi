namespace WorldPinball.Application.Features.Leaderboard.Public.Queries;

public sealed record GetLeaderboardQuery(
    Guid? IpId = null,
    Guid? ModeId = null,
    string? IpSlug = null,
    string? ModeCode = null,
    int Limit = 50
);
