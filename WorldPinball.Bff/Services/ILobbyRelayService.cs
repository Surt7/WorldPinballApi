namespace WorldPinball.Bff.Services;

public interface ILobbyRelayService
{
    Task EnsureWatchingLobbyAsync(Guid sessionId, CancellationToken cancellationToken);
}