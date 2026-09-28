namespace WorldPinball.Application.Interfaces.Services;

public interface IMatchmakingQueueService
{
    Task EnqueueAsync(string machineId, Guid playerId, CancellationToken ct = default);
    Task<Guid?> DequeueNextAsync(string machineId, CancellationToken ct = default);
    Task<bool> RemoveAsync(string machineId, Guid playerId, CancellationToken ct = default);
}