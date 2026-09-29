using Microsoft.AspNetCore.SignalR;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Api.Hubs;

public sealed class SignalRLobbyNotificationService(
    IHubContext<LobbyHub, ILobbyHubClient> hubContext
    ): ILobbyNotificationService
{
 
    public void NotifyLobbyUpdated(Guid sessionId)
        => hubContext.Clients.Group(sessionId.ToString());

    public async IAsyncEnumerable<Guid> SubscribeAsync(Guid sessionId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return sessionId;
        await Task.CompletedTask;
    }

    public async Task NotifyScoreSubmittedAsync(Guid sessionId, int slotNumber, long score, CancellationToken ct = default)
    {
        await hubContext.Clients.Group(sessionId.ToString())
            .ScoreSubmitted(slotNumber, score);
    }
    public async Task NotifySessionLaunchedAsync(Guid sessionId, CancellationToken ct = default)
    {
        await hubContext.Clients.Group(sessionId.ToString()).SessionLaunched();
    }
    public async Task NotifySessionEndedAsync(Guid sessionId, CancellationToken ct = default)
    {
        await hubContext.Clients.Group(sessionId.ToString()).SessionEnded(sessionId);
    }
    public async Task NotifySessionAbortedAsync(Guid sessionId, CancellationToken ct = default)
    {
        await hubContext.Clients.Group(sessionId.ToString()).SessionAborted(sessionId);
    }
}