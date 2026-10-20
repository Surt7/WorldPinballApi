using System.Text.Json;
using StackExchange.Redis;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Redis;

/// <summary>
/// High-performance in-memory leaderboard engine backed by Redis Sorted Sets (ZSET).
/// Provides sub-millisecond score updates and logarithmic O(log N) rank lookups (via skip-list internals),
/// shielding relational storage from heavy real-time leaderboard polling.
/// Partitions leaderboards dynamically by IP license and game mode (<c>leaderboard:{ipId}:{modeId}</c>) to avoid global contention.
/// </summary>
public class RedisRealtimeLeaderboardService : IRealtimeLeaderboardService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisRealtimeLeaderboardService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private static string GetKey(Guid ipId, Guid modeId) =>
        $"leaderboard:{ipId}:{modeId}";

    public async Task AddScoreAsync(
        Guid ipId,
        Guid modeId,
        Guid scoreEntryId,
        long scoreEntry,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipId, modeId);

        await db.SortedSetAddAsync(key, scoreEntryId.ToString(), scoreEntry);
    }


    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTopScoresAsync(
        Guid ipId,
        Guid modeId,
        int limit = 50,
        CancellationToken ct = default
    )
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipId, modeId);

        var entries = await db.SortedSetRangeByRankWithScoresAsync(
            key,
            start: 0,
            stop: limit - 1,
            order: Order.Descending
        );
        var result = new List<LeaderboardEntryDto>(entries.Length);
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            if (Guid.TryParse(entry.Element.ToString(), out var scoreEntryId) &&
                entry.Score is >= 0 and <= long.MaxValue)
            {
                // It could be long(entry.Score) but I'm cautious about casting and overflow
                var entryScore = Convert.ToInt64(entry.Score);
                result.Add(new LeaderboardEntryDto(scoreEntryId, entryScore, i + 1));
            }
        }

        return result;
    }


    /// <summary>
    /// Computes the exact real-time rank of a score entry within its game mode.
    /// Converts Redis zero-based index to standard 1-based human display rank (e.g., index 0 becomes rank 1).
    /// </summary>
    public async Task<long?> GetRankAsync(
        Guid ipId,
        Guid modeId,
        Guid scoreEntryId,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipId, modeId);

        var rankIndex = await db.SortedSetRankAsync(
            key,
            scoreEntryId.ToString(),
            Order.Descending
        );



        if (rankIndex is null)
        {
            return null;
        }

        return rankIndex.Value + 1;
    }


    public async Task<IReadOnlyList<LeaderboardRowDto>?> GetCachedLeaderboardAsync(
        Guid ipId,
        Guid modeId,
        int limit,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"leaderboard:dto:{ipId}:{modeId}:{limit}";
        var value = await db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;
        
        return JsonSerializer.Deserialize<IReadOnlyList<LeaderboardRowDto>>(value.ToString());
    }



    public async Task SetCachedLeaderboardAsync(
        Guid ipId,
        Guid modeId,
        int limit,
        IReadOnlyList<LeaderboardRowDto> rows,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"leaderboard:dto:{ipId}:{modeId}:{limit}";
        var json = JsonSerializer.Serialize(rows);
        var expiry = ttl ?? TimeSpan.FromSeconds(60);

        await db.StringSetAsync(key, json, expiry);
    }
}