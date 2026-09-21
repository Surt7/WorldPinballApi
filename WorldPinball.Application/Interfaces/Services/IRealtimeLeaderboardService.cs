using WorldPinball.Application.Features.Leaderboard.Public.Dtos;

namespace WorldPinball.Application.Interfaces.Services;


public record LeaderboardEntryDto(
    Guid ScoreEntryId,
    long Score,
    long Rank
);
public interface IRealtimeLeaderboardService
{
    Task AddScoreAsync(
        Guid ipId,
        Guid modeId,
        Guid scoreEntryId,
        long score,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<LeaderboardEntryDto>> GetTopScoresAsync(
        Guid ipId,
        Guid modeId,
        int limit = 50,
        CancellationToken ct = default
    );
    Task<long?> GetRankAsync(
        Guid ipId,
        Guid modeId,
        Guid scoreEntryId,
        CancellationToken ct = default
    );


    Task<IReadOnlyList<LeaderboardRowDto>?> GetCachedLeaderboardAsync(
        Guid ipId,
        Guid modeId,
        int limit,
        CancellationToken ct = default);
    
    Task SetCachedLeaderboardAsync(Guid ipId,
        Guid modeId,
        int limit,
        IReadOnlyList<LeaderboardRowDto> rows,
        TimeSpan? ttl = null,
        CancellationToken ct = default);

}