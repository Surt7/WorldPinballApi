using WorldPinball.Application.Features.GameSessions.Public.AbortSession.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.AbortSession;

public interface IAbortSessionHandler
{
    Task<AbortSessionResult> HandleAsync(AbortSessionCommand command, CancellationToken cancellationToken = default);
}