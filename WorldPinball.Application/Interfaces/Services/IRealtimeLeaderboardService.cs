namespace WorldPinball.Application.Interfaces.Services;


public record LeaderboardEntryDto(
    Guid ScoreEntryId,
    long Score,
    long Rank
);
public interface IRealtimeLeaderboardService
{
    Task AddScoreAsync(
        string ipSlug,
        string modeCode,
        Guid scoreEntryId,
        long score,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<LeaderboardEntryDto>> GetTopScoresAsync(
        string ipSlug,
        string modeCode,
        int limit = 50,
        CancellationToken ct = default
    );
    Task<long?> GetRankAsync(
        string ipSlug,
        string modeCode,
        Guid scoreEntryId,
        CancellationToken ct = default
    );
}