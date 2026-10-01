using WorldPinball.Protos.V1.GameSessions;

namespace WorldPinball.Bff.Services;

public interface ILobbyRelayService
{
    IReadOnlyCollection<SessionLobbyResponse> GetCurrentLobbies();
    Task EnsureWatchingLobbyAsync(Guid sessionId, CancellationToken cancellationToken);
 
    Task EnsureWatchingAllLobbiesAsync(CancellationToken cancellationToken);
}