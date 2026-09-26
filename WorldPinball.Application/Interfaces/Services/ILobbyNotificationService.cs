namespace WorldPinball.Application.Interfaces.Services;

public interface ILobbyNotificationService
{
    void NotifyLobbyUpdated(Guid sessionId);
    IAsyncEnumerable<Guid> SubscribeAsync(Guid sessionId, CancellationToken cancellationToken = default);
}