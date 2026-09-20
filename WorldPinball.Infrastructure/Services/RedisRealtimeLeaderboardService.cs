using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services;

public class RedisRealtimeLeaderboardService : IRealtimeLeaderboardService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisRealtimeLeaderboardService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private static string GetKey(string ipSlug, string modeCode) =>
        $"leaderboard:{ipSlug.ToLowerInvariant()}:{modeCode.ToLowerInvariant()}";

    public async Task AddScoreAsync(
        string ipSlug,
        string modeCode,
        Guid scoreEntryId,
        long scoreEntry,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipSlug, modeCode);

        await db.SortedSetAddAsync(key, scoreEntryId.ToString(), scoreEntry);
    }


    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTopScoresAsync(
        string ipSlug,
        string modeCode,
        int limit = 50,
        CancellationToken ct = default
    )
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipSlug, modeCode);

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
                // It could be long(entry.Score) but i'm cautious about casting and overflow
                var entryScore = Convert.ToInt64(entry.Score);
                result.Add(new LeaderboardEntryDto(scoreEntryId, entryScore, i + 1));
            }
        }

        return result;
    }


    public async Task<long?> GetRankAsync(
        string ipSlug,
        string modeCode,
        Guid scoreEntryId,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(ipSlug, modeCode);

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
    
}