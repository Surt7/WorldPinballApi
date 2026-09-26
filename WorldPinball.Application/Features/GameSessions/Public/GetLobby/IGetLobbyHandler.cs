using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Queries;

namespace WorldPinball.Application.Features.GameSessions.Public.GetLobby;

public interface IGetLobbyHandler
{
    Task<GetLobbyResult> HandleAsync(
        GetLobbyQuery query,
        CancellationToken cancellationToken = default
    );
}