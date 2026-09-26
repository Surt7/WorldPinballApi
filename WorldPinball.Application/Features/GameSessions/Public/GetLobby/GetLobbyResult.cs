using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Dtos;

namespace WorldPinball.Application.Features.GameSessions.Public.GetLobby;

public abstract record GetLobbyResult{

    private GetLobbyResult() {}
    
    public sealed record Success(SessionLobbyDto Lobby) : GetLobbyResult;
    public sealed record SessionNotFound(Guid SessionId) : GetLobbyResult;
};