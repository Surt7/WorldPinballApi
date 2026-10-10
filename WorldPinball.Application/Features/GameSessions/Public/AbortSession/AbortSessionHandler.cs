using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.AbortSession.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.GameSessions.Public.AbortSession;

public sealed class AbortSessionHandler(
    IGameSessionWriteRepository writeRepository,
    ILobbyNotificationService lobbyNotifier,
    TimeProvider timeProvider,
    ILogger<AbortSessionHandler> logger
) : IAbortSessionHandler
{
    public async Task<AbortSessionResult> HandleAsync(AbortSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await writeRepository.GetSessionWithMachineAsync(command.SessionId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning("Game session {SessionId} not found.", command.SessionId);
            return new AbortSessionResult.SessionNotFound(command.SessionId);
        }

        try
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            session.Abort(nowUtc);
        }
        catch (InvalidOperationException ex)
        {
            return new AbortSessionResult.CannotAbortSession(ex.Message);
        }

        await writeRepository.UpdateSessionAsync(session, cancellationToken);
        await lobbyNotifier.NotifySessionAbortedAsync(session.SessionId, cancellationToken);

        return new AbortSessionResult.Success(session.SessionId);
    }
}