using Microsoft.AspNetCore.SignalR;

namespace WorldPinball.Api.Hubs;


public sealed class LobbyHub : Hub<ILobbyHubClient>
{
    public async Task JoinLobby(string sessionId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    public async Task LeaveLobby(string sessionId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
}