using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Redis;

public sealed class RedisLiveTelemetryService(
    IConnectionMultiplexer redis,
    ILobbyNotificationService lobbyNotifier
) : ILiveTelemetryService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<long> IngestDeltaAsync(Guid sessionId, int slotNumber, long pointsDelta, string triggerEvent, CancellationToken token = default)
    {
        if (pointsDelta <= 0)
            throw new ArgumentOutOfRangeException(nameof(pointsDelta), "Points delta must be positive.");

        var key = $"telemetry:session:{sessionId}:slot:{slotNumber}";
        
        // 1. Atomically increment volatile score in Redis
        var currentTotal = await _db.StringIncrementAsync(key, pointsDelta);
        await _db.KeyExpireAsync(key, TimeSpan.FromHours(2));

        // 2. Broadcast live score to SignalR spectators
        await lobbyNotifier.NotifyScoreSubmittedAsync(sessionId, slotNumber, currentTotal, token);

        return currentTotal;
    }
}