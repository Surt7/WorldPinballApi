using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore;


/// <summary>
/// Orchestrates score submission workflow for completed game sessions.
/// Implements the Result pattern (returning a discriminated union of <see cref="SubmitScoreResult"/>)
/// instead of throwing exceptions for predictable domain outcomes (e.g., mismatched device, expired session).
/// Leverages <see cref="TimeProvider"/> for deterministic clock abstraction during unit testing.
/// Persists entries to relational storage before updating high-throughput Redis leaderboards and broadcasting via SignalR.
/// </summary>
public sealed class SubmitScoreHandler(
    IGameSessionWriteRepository writeRepository,
    IRealtimeLeaderboardService realtimeLeaderboardService,
    ILobbyNotificationService lobbyNotifier,
    ILogger<SubmitScoreHandler> logger,
    TimeProvider timeProvider
    ) : ISubmitScoreHandler
{
    private static readonly EventId SessionNotFound = new EventId(2400, nameof(SessionNotFound));
    private static readonly EventId SessionAlreadyEnded = new EventId(2401, nameof(SessionAlreadyEnded));
    private static readonly EventId SessionNotStarted = new EventId(2402, nameof(SessionNotStarted));
    private static readonly EventId DeviceMisMatch = new EventId(2403, nameof(DeviceMisMatch));
    private static readonly EventId SlotNotFound = new EventId(2404, nameof(SlotNotFound));

    public async Task<SubmitScoreResult> HandleAsync(SubmitScoreCommand command, CancellationToken token = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var gameSession = await writeRepository.GetSessionWithMachineAsync(command.SessionId, token);
     
        if (gameSession is null)
        {
            logger.LogWarning(
                SessionNotFound,
                "Session {SessionId} could not be found or is not active.",
                command.SessionId);
            return new SubmitScoreResult.SessionNotFound(command.SessionId);
        }
        
 
        
        if (gameSession.DeviceId != command.DeviceId)
        {
            logger.LogWarning(
                DeviceMisMatch,
                "Actual device {ActualDeviceId} doesn't match expected device {ExpectedDeviceId}.",
                command.DeviceId,
                gameSession.DeviceId);
            return new SubmitScoreResult.DeviceMisMatch(gameSession.DeviceId, command.DeviceId);
        }
        
        if (gameSession.State == GameSessionState.Ended)
        {
            logger.LogWarning(
                SessionAlreadyEnded,
                "Session {SessionId} is already ended.",
                gameSession.SessionId);
            return new SubmitScoreResult.SessionAlreadyEnded(gameSession.SessionId);
        }

        if (gameSession.State is not (GameSessionState.Started or GameSessionState.InGame or GameSessionState.Resumed or GameSessionState.Paused))
        {
            logger.LogWarning(SessionNotStarted, "Session {SessionId} is not started.", command.SessionId);
            return new SubmitScoreResult.SessionNotStarted(command.SessionId);
        }
        
        
        if (command.Scores.Count == 0)
        {
            return new SubmitScoreResult.EmptyScores();
        }

        // Validate all submitted slots exist in the session
        var scoreEntries = new List<ScoreEntry>();
        foreach (var scoreDto in command.Scores)
        {
            var player = gameSession.Players.FirstOrDefault(p => p.SlotNumber == scoreDto.SlotNumber);
            if (player is null)
            {
                logger.LogWarning(
                    SlotNotFound,
                    "Slot {SlotNumber} not found in session {SessionId}.",
                    scoreDto.SlotNumber,
                    gameSession.SessionId);
                return new SubmitScoreResult.SlotNotFound(scoreDto.SlotNumber);
            }

            var scoreEntry = new ScoreEntry(
                gameSession.SessionId,
                scoreDto.SlotNumber,
                gameSession.DeviceId,
                gameSession.MachineId,
                gameSession.Machine?.IpId ?? Guid.NewGuid(),
                gameSession.ModeId,
                scoreDto.Score,
                nowUtc,
                player.UserId?.ToString()
            );
            scoreEntries.Add(scoreEntry);
        }

        // End the session now that all scores are recorded
        gameSession.End(nowUtc);

        await writeRepository.SaveScoreEntriesAsync(gameSession, scoreEntries, token);
        
        foreach (var scoreEntry in scoreEntries)
        {
            await realtimeLeaderboardService.AddScoreAsync(
                scoreEntry.IpId,
                scoreEntry.ModeId,
                scoreEntry.ScoreEntryId,
                scoreEntry.Score,
                token
            );
            
            await lobbyNotifier.NotifyScoreSubmittedAsync(
                gameSession.SessionId,
                scoreEntry.SlotNumber,
                scoreEntry.Score,
                token
            );
        }

        await lobbyNotifier.NotifySessionEndedAsync(gameSession.SessionId, token);

        return new SubmitScoreResult.Success(scoreEntries.Select(s => s.ScoreEntryId).ToList());
    }
}