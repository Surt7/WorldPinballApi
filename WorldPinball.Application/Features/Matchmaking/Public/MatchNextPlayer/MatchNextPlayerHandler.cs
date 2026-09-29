using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer;

public sealed class MatchNextPlayerHandler(
    IGameSessionWriteRepository sessionWriteRepo,
    IMatchmakingQueueService queueService,
    ILobbyNotificationService lobbyNotifier,
    ILogger<MatchNextPlayerHandler> logger
    ):IMatchNextPlayerHandler
{
  
    private static readonly EventId NoMachinesFound = new(2900, nameof(NoMachinesFound));
    private static readonly EventId NoPlayerInQueue = new(2901, nameof(NoPlayerInQueue));
    private static readonly EventId SessionIsFull = new(2902, nameof(SessionIsFull));

    public async Task<MatchNextPlayerResult> HandleAsync(MatchNextPlayerCommand command, CancellationToken cancellationToken = default)
    {

        var session = await sessionWriteRepo.GetSessionWithMachineAsync(command.SessionId, cancellationToken);

        if (session is null)
        {
            logger.LogWarning(
                NoMachinesFound,
                "Game session : {SessionId}  could not be found.",
                command.SessionId
            );
            
            return new MatchNextPlayerResult.SessionNotFound(command.SessionId);
        }

        if (session.Players.Count >= 4)
        {
            logger.LogWarning(
                SessionIsFull,
                "Game session : {SessionId}  is full.",
                command.SessionId
            );
            
            return new MatchNextPlayerResult.SessionFull(command.SessionId);
        }
        
        var resRedis = await queueService.DequeueNextAsync(command.MachineId.ToString(), cancellationToken);
        
        if (resRedis is null)
        {
            logger.LogWarning(
                NoPlayerInQueue,
                " Queue from : {SessionId}  could not be found.",
                command.SessionId
            );
            
            return new MatchNextPlayerResult.NoPlayerInQueue();
        
        }
        // We search any available slot (we can use enumerable as int is a primitive unstead of a tedious loop)
        var nextSlot = Enumerable.Range(1, 4).First(slot => session.Players.All(p => p.SlotNumber != slot));
        session.AddPlayer(nextSlot, resRedis.Value);

        if (session.State == GameSessionState.SearchingPlayer && session.Players.Count >= 2)
        {
            session.SetMatchFound(DateTime.UtcNow);
            await lobbyNotifier.NotifyMatchFoundAsync(session.SessionId, session.MachineId, cancellationToken);
        }

        await sessionWriteRepo.UpdateSessionAsync(session, cancellationToken);

        return new MatchNextPlayerResult.Success(
            resRedis.Value, 
            nextSlot, 
            session.State == GameSessionState.MatchFound);
    }
}