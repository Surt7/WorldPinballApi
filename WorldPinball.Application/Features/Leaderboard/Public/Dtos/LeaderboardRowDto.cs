namespace WorldPinball.Application.Features.Leaderboard.Public.Dtos;

public sealed record LeaderboardRowDto(
    Guid ScoreEntryId,
    string IpSlug,
    string ModeCode,
    int? Score,
    DateTime? AchievedAtUtc,
    string UserId,
    Guid MachineId
);
