using WorldPinball.Application.Features.GameSessions.Public.Start.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.Start;

public interface IStartGameSessionHandler
{
    Task<StartGameSessionResult> HandleAsync(
        StartGameSessionCommand command,
        CancellationToken cancellationToken = default
    );
}