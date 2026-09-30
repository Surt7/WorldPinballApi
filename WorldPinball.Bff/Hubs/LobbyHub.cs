using Microsoft.AspNetCore.SignalR;
using WorldPinball.Bff.Services;
namespace WorldPinball.Bff.Hubs;

public sealed class LobbyHub(ILobbyRelayService relayService) : Hub
{
    public async Task JoinLobby(string sessionId)
    {

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        if (Guid.TryParse(sessionId, out var parsedId))
        {
            await relayService.EnsureWatchingLobbyAsync(parsedId, Context.ConnectionAborted);
        }
    }

    public async Task LeaveLobby(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }
}