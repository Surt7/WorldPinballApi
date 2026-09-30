using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WorldPinball.Bff.Hubs;


using WorldPinball.Protos.V1.GameSessions;

namespace WorldPinball.Bff.Services;
public sealed class LobbyRelayService(
    GameSessionService.GameSessionServiceClient grpcClient,
    IHubContext<LobbyHub> hubContext,
    ILogger<LobbyRelayService> logger
    ) :
    ILobbyRelayService
{
    private readonly ConcurrentDictionary<Guid, byte> _activeSessions = new();

    public async Task EnsureWatchingLobbyAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        
        // Atomic check-and-insert: if already being watched by another client,
        // skip starting a new stream.
        // Gently reminder :  Hashset or normal Dictionary<TKey, TValue> is not thread safe.
        if (!_activeSessions.TryAdd(sessionId, 0))
            return;

        _ = Task.Run(async () =>
        {

            try
            {

                var request = new WatchLobbyRequest { SessionId = sessionId.ToString() };
                using var call = grpcClient.WatchLobby(request,
                    cancellationToken: cancellationToken);

                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    var lobby = call.ResponseStream.Current;
                    await hubContext.Clients.Group(sessionId.ToString())
                        .SendAsync("LobbyStateUpdated", lobby, cancellationToken);
                }

            } // "When / is not" filter prevent to pollute log for a normal behavior on token cancellation call source.
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error while watching lobby stream for session {SessionId}", sessionId);
            }
            finally
            {
                _activeSessions.TryRemove(sessionId, out _); 
            }


        }, cancellationToken);



        await Task.CompletedTask;
    }
}