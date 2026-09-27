using Microsoft.AspNetCore.SignalR;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Api.Hubs;

public sealed class SignalRLobbyNotificationService(
    IHubContext<LobbyHub, ILobbyHubClient> hubContext): ILobbyNotificationService
{
 
    public void NotifyLobbyUpdated(Guid sessionId)
        => hubContext.Clients.Group(sessionId.ToString());

    public async IAsyncEnumerable<Guid> SubscribeAsync(Guid sessionId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return sessionId;
        await Task.CompletedTask;
    }
}