using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.RecordBallScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.GameSessions.Public.RecordBallScore;

public sealed class RecordBallScoreHandler(
    IGameSessionWriteRepository writeRepository,
    ILobbyNotificationService lobbyNotifier,
    TimeProvider timeProvider,
    ILogger<RecordBallScoreHandler> logger
) : IRecordBallScoreHandler
{
    private static readonly EventId SessionNotFound = new(2500, nameof(SessionNotFound));
    private static readonly EventId DeviceMismatch = new(2501, nameof(DeviceMismatch));
    
    public async Task<RecordBallScoreResult> HandleAsync(RecordBallScoreCommand command,
        CancellationToken token = default)
    {
        var session = await writeRepository.GetSessionWithMachineAsync(command.SessionId, token);
        if (session is null)
        {
            logger.LogWarning(SessionNotFound,
                "Session {SessionId} not found.", command.SessionId);
            return new RecordBallScoreResult.SessionNotFound(command.SessionId);
        }

        if (session.DeviceId != command.DeviceId)
        {
            logger.LogWarning(DeviceMismatch, 
                "Device {Actual} mismatch for session {SessionId}.", command.DeviceId, command.SessionId);
            return new RecordBallScoreResult.DeviceMisMatch(session.DeviceId, command.DeviceId);
        }
        
        
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        try
        {
            
            // 1. Record ball checkpoint in aggregate root
            session.RecordBallScore(command.SlotNumber, command.BallNumber, command.Score, nowUtc);
        
            
            // 2. Persist updated session
            await writeRepository.UpdateSessionAsync(session, token);
            
            
            // 3. Broadcast real-time score update to SignalR lobby/spectators
            await lobbyNotifier.NotifyScoreSubmittedAsync(session.SessionId, command.SlotNumber, command.Score, token);
            return new RecordBallScoreResult.Success(session.SessionId, command.SlotNumber, command.BallNumber, command.Score);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            logger.LogWarning(ex, "Cannot record ball score for session {SessionId}: {Message}", command.SessionId, ex.Message);
            return new RecordBallScoreResult.CannotRecordBallScore(ex.Message);
        }
    }
    
    
    
}