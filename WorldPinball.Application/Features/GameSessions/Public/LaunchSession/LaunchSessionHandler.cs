using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.GameSessions.Public.LaunchSession;

public sealed class LaunchSessionHandler(
    IGameSessionWriteRepository writeRepository,
    ILobbyNotificationService lobbyNotifier,
    TimeProvider timeProvider,
    ILogger<LaunchSessionHandler> logger
) : ILaunchSessionHandler
{
    private static readonly EventId SessionNotFound = new EventId(2800, nameof(SessionNotFound));

    public async Task<LaunchSessionResult> HandleAsync(LaunchSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await writeRepository.GetSessionWithMachineAsync(command.SessionId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning(SessionNotFound, "Game session {SessionId} not found.", command.SessionId);
            return new LaunchSessionResult.SessionNotFound(command.SessionId);
        }

        try
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            session.Launch(nowUtc);
        }
        catch (InvalidOperationException ex)
        {
            return new LaunchSessionResult.CannotStartSession(ex.Message);
        }

        await writeRepository.UpdateSessionAsync(session, cancellationToken);
        await lobbyNotifier.NotifySessionLaunchedAsync(session.SessionId, cancellationToken);

        return new LaunchSessionResult.Success(session.SessionId);
    }
}
