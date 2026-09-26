using WorldPinball.Application.Features.GameSessions.Public.LaunchSession.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.LaunchSession;

public interface ILaunchSessionHandler
{
    Task<LaunchSessionResult> HandleAsync(LaunchSessionCommand command, CancellationToken cancellationToken = default);
}
