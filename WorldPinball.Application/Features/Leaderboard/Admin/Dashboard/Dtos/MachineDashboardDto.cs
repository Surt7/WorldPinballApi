

namespace WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;

public sealed record MachineDashboardDto(
    Guid MachineId,
    string SerialNumber,
    int TotalGamesPlayed,
    long HighScore,
    long AverageScore,
    int TotalUniquePlayers,
    DateTime? LastPlayedAtUtc
);
