using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSesions.Public.Start.Commands;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.GameSesions.Public.Start;

public sealed class StartGameSessionHandler(
    IGameSessionReadRepository readRepository,
    IGameSessionWriteRepository writeRepository,
    ILogger<StartGameSessionHandler> logger,
    TimeProvider timeProvider
) : IStartGameSessionHandler
{
    public Task<StartGameSessionResult> HandleAsync(
        StartGameSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
