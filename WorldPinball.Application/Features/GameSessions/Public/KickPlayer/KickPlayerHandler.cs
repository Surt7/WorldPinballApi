using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.KickPlayer;

public sealed class KickPlayerHandler(
    IGameSessionWriteRepository writeRepository,
    ILogger<KickPlayerHandler> logger):IKickPlayerHandler
{
    private static readonly EventId SessionNotFound = new EventId(2600, nameof(SessionNotFound));
    private static readonly EventId InvalidSlot = new EventId(2601, nameof(InvalidSlot));
    private static readonly EventId PlayerNotFound = new EventId(2602, nameof(PlayerNotFound));
    private static readonly EventId CannotRemovePlayerActiveSession = 
        new EventId(2603, nameof(CannotRemovePlayerActiveSession));

    
    public async Task<KickPlayerResult> HandleAsync(
        KickPlayerCommand command,
        CancellationToken cancellationToken = default
        )
    {
        if (command is { SlotNumber: < 1 or > 4 })
        {
            logger.LogWarning(
                InvalidSlot,
                "Slot {Slot} in session {SessionId} is invalid.",
                command.SlotNumber, command.SessionId);
            return new KickPlayerResult.InvalidSlot(command.SlotNumber);
        }
        
        
        var session = await writeRepository.GetSessionWithMachineAsync(
            command.SessionId,
            cancellationToken);
        
        
        if (session is null)
        {
            logger.LogWarning(
                SessionNotFound,
                "Game session {SessionId} could not be found.",
                command.SessionId);
            return new KickPlayerResult.SessionNotFound(command.SessionId);
        }
        
        
        if (session.State is not (
            GameSessionState.Created
            or GameSessionState.Pending
            or GameSessionState.SearchingPlayer
            or GameSessionState.MatchFound
            or GameSessionState.Joined))
        {
            logger.LogWarning(
                CannotRemovePlayerActiveSession,
                "Cannot kick player from session {SessionId} in state {State}.",
                session.SessionId, session.State);
            return new KickPlayerResult.CannotRemovePlayerActiveSession(session.SessionId);
        }
        
        if (!session.Players.Any(p => p.SlotNumber == command.SlotNumber))
        {
            logger.LogWarning(
                PlayerNotFound,
                "No player found in slot {Slot} for session {SessionId}.",
                command.SlotNumber, session.SessionId);
            return new KickPlayerResult.PlayerNotFound(command.SlotNumber);
        }
        session.RemovePlayer(command.SlotNumber);
        await writeRepository.UpdateSessionAsync(session, cancellationToken);
        logger.LogInformation(
            "Player removed from slot {Slot} in session {SessionId}.",
            command.SlotNumber, session.SessionId);
        return new KickPlayerResult.Success(session.SessionId, command.SlotNumber);
    }
    
    
    
}