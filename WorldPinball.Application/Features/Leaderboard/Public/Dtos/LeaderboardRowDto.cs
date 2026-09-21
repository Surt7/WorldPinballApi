namespace WorldPinball.Application.Features.Leaderboard.Public.Dtos;

public sealed record LeaderboardRowDto(
    int Rank,
    Guid ScoreEntryId,
    string PlayerInitials,
    string PlayerName,
    long Score,
    DateTime AchievedAtUtc,
    string IpName,
    string ModeName
);