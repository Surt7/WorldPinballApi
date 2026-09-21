namespace WorldPinball.Api.Contracts.Leaderboard;

public sealed record GetLeaderboardRequest(
    Guid? IpId = null,
    Guid? ModeId = null,
    string? IpSlug = null,
    string? ModeCode = null,
    int Limit = 50
);