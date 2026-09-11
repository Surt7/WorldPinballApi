using WorldPinball.Application.Features.GameSesions.Public.Start.Commands;

namespace WorldPinball.Application.Features.GameSesions.Public.Start;

public interface IStartGameSessionHandler
{
    Task<StartGameSessionResult> HandleAsync(
        StartGameSessionCommand command,
        CancellationToken cancellationToken = default
    );
}