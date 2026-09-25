using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore;

public sealed class SubmitScoreHandler(
    IGameSessionWriteRepository writeRepository,
    IRealtimeLeaderboardService realtimeLeaderboardService,
    ILogger<SubmitScoreHandler> logger,
    TimeProvider timeProvider
    ) : ISubmitScoreHandler
{
    private static readonly EventId SessionNotFound = new EventId(2400, nameof(SessionNotFound));
    private static readonly EventId SessionAlreadyEnded = new EventId(2401, nameof(SessionAlreadyEnded));
    private static readonly EventId DeviceMisMatch = new EventId(2402, nameof(DeviceMisMatch));
    private static readonly EventId SlotNotFound = new EventId(2403, nameof(SlotNotFound));

    public async Task<SubmitScoreResult> HandleAsync(SubmitScoreCommand command, CancellationToken token = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var gameSession = await writeRepository.GetSessionWithMachineAsync(command.SessionId, token);

        if (gameSession is null)
        {
            logger.LogWarning(
                SessionNotFound,
                "Device {DeviceId} could not be found.",
                command.DeviceId);
            return new SubmitScoreResult.SessionNotFound(command.DeviceId);
        }

        if (gameSession.DeviceId != command.DeviceId)
        {
            logger.LogWarning(
                DeviceMisMatch,
                "Actual device {DeviceId} doesn't match the expected device.",
                gameSession.SessionId);
            return new SubmitScoreResult.DeviceMisMatch(command.DeviceId, gameSession.SessionId);
        }
        
        if (gameSession.State == GameSessionState.Ended)
        {
            logger.LogWarning(
                SessionAlreadyEnded,
                "Device {DeviceId} Already ended.",
                gameSession.SessionId);
            return new SubmitScoreResult.SessionAlreadyEnded(gameSession.SessionId);
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

        foreach (var scoreEntry in scoreEntries)
        {
            await writeRepository.SaveScoreEntryAsync(gameSession, scoreEntry, token);
            await realtimeLeaderboardService.AddScoreAsync(
                scoreEntry.IpId,
                scoreEntry.ModeId,
                scoreEntry.ScoreEntryId,
                scoreEntry.Score,
                token
            );
        }

        return new SubmitScoreResult.Success(scoreEntries.Select(s => s.ScoreEntryId).ToList());
    }
}