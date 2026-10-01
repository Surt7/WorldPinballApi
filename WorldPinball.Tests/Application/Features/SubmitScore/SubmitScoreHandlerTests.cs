using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.SubmitScore;

public class SubmitScoreHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        var logger = new FakeLogger<SubmitScoreHandler>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);
        
        var command = new SubmitScoreCommand(Guid.NewGuid(), Guid.NewGuid(), [new SlotScoreDto(1, 50000)]);
        var sut = new SubmitScoreHandler(
            writeRepository,
            realtimeLeaderboardService,
            lobbyNotifier,
            logger,
            TimeProvider.System);
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionNotFound>();
    }

    [Fact]
    public async Task HandleAsync_WhenSessionIsEnded_ShouldReturnResultSessionAlreadyEnded()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.AddPlayer(1, Guid.NewGuid());
        gameSession.Launch(start.UtcDateTime);
        gameSession.End(end.UtcDateTime);
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var command = new SubmitScoreCommand(Guid.NewGuid(), deviceId, [new SlotScoreDto(1, 50000)]);
        
        var sut = new SubmitScoreHandler(
            writeRepository,
            realtimeLeaderboardService,
            lobbyNotifier,
            logger,
            TimeProvider.System);
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionAlreadyEnded>();
    }

    [Fact]
    public async Task HandleAsync_DeviceDoesntMatch_ReturnResultDeviceMisMatch()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2;
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.AddPlayer(slotNumber, userId, "AAA");
        gameSession.Launch(start.UtcDateTime);
        
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(Guid.NewGuid(), Guid.NewGuid(), [new SlotScoreDto(slotNumber, 50000)]);
        
        var sut = new SubmitScoreHandler(
            writeRepository,
            realtimeLeaderboardService,
            lobbyNotifier,
            logger,
            TimeProvider.System);
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
 
        result.Should().BeOfType<SubmitScoreResult.DeviceMisMatch>();
    }

    [Fact]
    public async Task HandleAsync_WhenSlotNotFound_ReturnResultSlotNotFound()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2;
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.AddPlayer(slotNumber, userId, "AAA");
        gameSession.Launch(start.UtcDateTime);
        
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(gameSession.SessionId, deviceId, [new SlotScoreDto(4, 50000)]);
        
        var sut = new SubmitScoreHandler(
            writeRepository,
            realtimeLeaderboardService,
            lobbyNotifier,
            logger,
            TimeProvider.System);
     
        var result = await sut.HandleAsync(command, CancellationToken.None);

        result.Should().BeOfType<SubmitScoreResult.SlotNotFound>();
    }

    [Fact]
    public async Task RequestIsValid_ShouldReturnResultSuccess()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2;
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.AddPlayer(slotNumber, userId, "AAA");
        gameSession.Launch(start.UtcDateTime);
        
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(gameSession.SessionId, deviceId, [new SlotScoreDto(slotNumber, 50000)]);
        
        var sut = new SubmitScoreHandler(writeRepository,
            realtimeLeaderboardService, 
            lobbyNotifier,
            logger,
            TimeProvider.System);
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        await realtimeLeaderboardService.Received(1).AddScoreAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            50000,
            Arg.Any<CancellationToken>()
        );
        
        gameSession.State.Should().Be(GameSessionState.Ended);
        result.Should().BeOfType<SubmitScoreResult.Success>();
    }
    
    [Fact]
    public async Task HandleAsync_WhenMultipleSlotsSubmitted_ShouldSaveAllScoresAndEndSession()
    {
        // Arrange
        var lobbyNotifier = Substitute.For<ILobbyNotificationService>();
        var logger = new FakeLogger<SubmitScoreHandler>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var now = DateTime.UtcNow;

        var gameSession = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", now);
        gameSession.AddPlayer(1, Guid.NewGuid(), "AAA");
        gameSession.AddPlayer(2, Guid.NewGuid(), "BBB");
        gameSession.Launch(now);

        writeRepository.GetSessionWithMachineAsync(
                gameSession.SessionId, 
                Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var scores = new List<SlotScoreDto>
        {
            new(1, 1500000),
            new(2, 850000)
        };
        var command = new SubmitScoreCommand(gameSession.SessionId, gameSession.DeviceId, scores);
        var sut = new SubmitScoreHandler(
            writeRepository, 
            realtimeLeaderboardService,
            lobbyNotifier,
            logger,
            TimeProvider.System);

        // Act
        var result = await sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SubmitScoreResult.Success>();
        gameSession.State.Should().Be(GameSessionState.Ended);
        await realtimeLeaderboardService.Received(2).AddScoreAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), 
            Arg.Any<Guid>(), 
            Arg.Any<long>(), Arg.Any<CancellationToken>());
        
        await lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(
            gameSession.SessionId, 1, 1500000, Arg.Any<CancellationToken>());
        await lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(
            gameSession.SessionId, 2, 850000, Arg.Any<CancellationToken>());
        await lobbyNotifier.Received(1).NotifySessionEndedAsync(
            gameSession.SessionId, Arg.Any<CancellationToken>());
    }
}
