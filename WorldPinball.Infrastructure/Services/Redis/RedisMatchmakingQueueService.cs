using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Redis;




public class RedisMatchmakingQueueService:IMatchmakingQueueService
{
    private readonly IDatabase _db;
    private readonly TimeProvider _timeProvider;
    
    public RedisMatchmakingQueueService(IConnectionMultiplexer redis, TimeProvider timeProvider)
    {
        _db = redis.GetDatabase();
        _timeProvider = timeProvider;
    }
    
    
    
    public async Task EnqueueAsync(string machineId, Guid playerId, CancellationToken ct = default)
    {
        var score = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        await _db.SortedSetAddAsync(
            $"matchmaking:queue:{machineId}", 
            playerId.ToString(), 
            score);
        
    }

    public async Task<Guid?> DequeueNextAsync(string machineId, CancellationToken ct = default)
    {
       
        var entry = await _db.SortedSetPopAsync(
            $"matchmaking:queue:{machineId}",
            Order.Ascending);

         if (entry is null || entry.Value.Element.IsNullOrEmpty)
            return null;

         return Guid.Parse(entry.Value.Element.ToString());
         
    }

    public async Task<bool> RemoveAsync(string machineId, Guid playerId, CancellationToken ct = default)
    {
        return await _db.SortedSetRemoveAsync(
            $"matchmaking:queue:{machineId}",
            playerId.ToString());
        
    }
}