using System.Text.Json;
using StackExchange.Redis;

namespace WorldPinball.Api.Filters.Idempotency;

public sealed class HybridIdempotencyStore(
    InMemoryIdempotencyStore localStore,
    ILogger<HybridIdempotencyStore> logger,
    IConnectionMultiplexer? redis = null
) : IIdempotencyStore
{
    public async Task<IdempotentResponse?> GetAsync(string key, CancellationToken ct = default)
    {
        var localCached = await localStore.GetAsync(key, ct);
        if (localCached is not null)
        {
            return localCached;
        }

        if (redis is null || !redis.IsConnected)
        {
            return null;
        }

        try
        {
            var redisVal = await redis.GetDatabase()
                .StringGetAsync($"idempotency:{key}");

            if (redisVal.HasValue)
            {
                var response = JsonSerializer
                    .Deserialize<IdempotentResponse>(redisVal.ToString());
                if (response is not null)
                {
                    await localStore.SetAsync(key, response, 
                        TimeSpan.FromMinutes(30), ct);
                    return response;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, 
                "Redis lookup failed for key {Key}," +
                " falling back to local memory", key);
        }
        return null;
    }

    public async Task SetAsync(string key, IdempotentResponse response,
        TimeSpan duration, CancellationToken ct = default)
    {
        await localStore.SetAsync(key, response, duration, ct);
        
        if (redis is null || !redis.IsConnected) return;

        try
        {
            var json = JsonSerializer.Serialize(response);
            await redis.GetDatabase()
                .StringSetAsync($"idempotency:{key}", json, duration);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis write failed for key {Key}," +
                                  " continuing with local memory only", key);
        }
    }
}