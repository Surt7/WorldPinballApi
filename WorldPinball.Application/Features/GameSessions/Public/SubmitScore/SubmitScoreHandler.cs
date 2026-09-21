using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.Start;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore;

public sealed class SubmitScoreHandler(
    IGameSessionReadRepository readRepository,
    IGameSessionWriteRepository writeRepository,
    IRealtimeLeaderboardService realtimeLeaderboardService,
    ILogger<SubmitScoreHandler> logger,
    TimeProvider timeProvider
    ):ISubmitScoreHandler
{
    private static readonly EventId SessionNotFound = new EventId(2400, nameof(SessionNotFound));
    private static readonly EventId SessionAlreadyEnded = new EventId(2401, nameof(SessionAlreadyEnded));
    private static readonly EventId DeviceMisMatch = new EventId(2402, nameof(DeviceMisMatch));
    private static readonly EventId SlotNotFound = new EventId(2402, nameof(SlotNotFound));
    public  async Task<SubmitScoreResult> HandleAsync(SubmitScoreCommand command, CancellationToken token = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var gameSession = await writeRepository.GetSessionWithMachineAsync(command.SessionId, token);

        if (gameSession is null)
        {
            logger.LogWarning(
                SessionNotFound,
                "Device {DeviceId}  could not be found.",
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
        
        // We retrive all players from a game session
        var player = gameSession.Players.FirstOrDefault(p => p.SlotNumber == command.SlotNumber);
        if (player is null)
        {
            logger.LogWarning(
                SlotNotFound,
                "Device {DeviceId} Already ended.",
                command.SlotNumber);
            return new SubmitScoreResult.SlotNotFound(command.SlotNumber);
        }
        gameSession.End(nowUtc);
        var scoreEntry = new ScoreEntry(
            gameSession.SessionId,
            command.SlotNumber,
            gameSession.DeviceId,
            gameSession.MachineId,
            gameSession.Machine?.IpId ?? Guid.NewGuid(), // Machine that belong to a license
            gameSession.ModeId,
            command.Score,
            nowUtc,
            player?.UserId?.ToString()
        );
        
        await writeRepository.SaveScoreEntryAsync(gameSession, scoreEntry, token);
        await realtimeLeaderboardService.AddScoreAsync(
            scoreEntry.IpId,
            scoreEntry.ModeId,
            scoreEntry.ScoreEntryId,
            command.Score,
            token
        );
        return new SubmitScoreResult.Success(scoreEntry.ScoreEntryId);
    }
}