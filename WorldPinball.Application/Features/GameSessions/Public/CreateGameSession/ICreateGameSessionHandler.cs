using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;

public interface ICreateGameSessionHandler
{
    Task<CreateGameSessionResult> HandleAsync(
        CreateGameSessionCommand command,
        CancellationToken cancellationToken = default
    );
}