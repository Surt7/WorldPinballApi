namespace WorldPinball.Application.Interfaces.Services;

public interface ILobbyNotificationService
{
    void NotifyLobbyUpdated(Guid sessionId);
    
    IAsyncEnumerable<Guid> SubscribeAsync(Guid sessionId,
        CancellationToken cancellationToken = default);
    
    Task NotifyScoreSubmittedAsync(Guid sessionId, int slotNumber,
        long score, CancellationToken ct = default);
    
    Task NotifySessionLaunchedAsync(Guid sessionId,
        CancellationToken ct = default);
    
    Task NotifySessionEndedAsync(Guid sessionId,
        CancellationToken ct = default);
    
    Task NotifySessionAbortedAsync(Guid sessionId,
        CancellationToken ct = default);

    Task NotifyMatchFoundAsync(Guid sessionId, Guid machineId,
        CancellationToken ct = default);
}