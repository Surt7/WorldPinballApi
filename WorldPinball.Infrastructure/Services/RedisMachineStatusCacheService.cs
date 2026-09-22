using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services;

public class RedisMachineStatusCacheService:IMachineStatusCacheService
{
    private readonly IConnectionMultiplexer _redis;
    
    public RedisMachineStatusCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    public async Task<string?> GetStatusAsync(Guid machineId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"machine:status:{machineId}";
        var value =await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task SetStatusAsync(Guid machineId, string status, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"machine:status:{machineId}";

        await db.StringSetAsync(key, status, ttl);
        
        
    }

    public async Task InvalidateStatusAsync(Guid machineId, CancellationToken ct = default)
    {
        
        var db = _redis.GetDatabase();
        var key = $"machine:status:{machineId}";
        
        await db.KeyDeleteAsync(key);
   
    }
}