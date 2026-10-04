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
    private readonly IGameSessionWriteRepository _writeRepository = Substitute.For<IGameSessionWriteRepository>();
    private readonly IRealtimeLeaderboardService _realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
    private readonly ILobbyNotificationService _lobbyNotifier = Substitute.For<ILobbyNotificationService>();
    private readonly FakeLogger<SubmitScoreHandler> _logger = new();

    private readonly SubmitScoreHandler _sut;
    
    public SubmitScoreHandlerTests()
    {
        _sut = new SubmitScoreHandler(_writeRepository, _realtimeLeaderboardService, _lobbyNotifier, _logger, TimeProvider.System);
    }
    
    [Fact]
    public async Task HandleAsync_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        _writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);
        
        var command = new SubmitScoreCommand(Guid.NewGuid(), Guid.NewGuid(), [new SlotScoreDto(1, 50000)]);
        
        var result = await _sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionNotFound>();
    }

    [Fact]
    public async Task HandleAsync_WhenSessionIsEnded_ShouldReturnResultSessionAlreadyEnded()
    {
        var now = DateTime.UtcNow;
        var gameSession = new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            now
        );
        gameSession.AddPlayer(1, Guid.NewGuid());
        gameSession.Launch(now);
        gameSession.End(now.AddMinutes(10));
        _writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(Guid.NewGuid(), gameSession.DeviceId, [new SlotScoreDto(1, 50000)]);
        
        var result = await _sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionAlreadyEnded>();
    }

    [Fact]
    public async Task HandleAsync_DeviceDoesntMatch_ReturnResultDeviceMisMatch()
    {
        var now = DateTime.UtcNow;
        var gameSession = new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            now
        );
        gameSession.AddPlayer(2, Guid.NewGuid(), "AAA");
        gameSession.Launch(now);
        
        _writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var differentDeviceId = Guid.NewGuid();
        var command = new SubmitScoreCommand(gameSession.SessionId, differentDeviceId, [new SlotScoreDto(2, 50000)]);
        
        var result = await _sut.HandleAsync(command, CancellationToken.None);
 
        result.Should().BeOfType<SubmitScoreResult.DeviceMisMatch>();
    }

    [Fact]
    public async Task HandleAsync_WhenSlotNotFound_ReturnResultSlotNotFound()
    {
        var now = DateTime.UtcNow;
        var gameSession = new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            now
        );
        gameSession.AddPlayer(2, Guid.NewGuid(), "AAA");
        gameSession.Launch(now);
        
        _writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(gameSession.SessionId, gameSession.DeviceId, [new SlotScoreDto(4, 50000)]);
     
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        result.Should().BeOfType<SubmitScoreResult.SlotNotFound>();
    }

    [Fact]
    public async Task RequestIsValid_ShouldReturnResultSuccess()
    {
        var now = DateTime.UtcNow;
        var gameSession = new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            now
        );
        gameSession.AddPlayer(2, Guid.NewGuid(), "AAA");
        gameSession.Launch(now);
        
        _writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var command = new SubmitScoreCommand(gameSession.SessionId, gameSession.DeviceId, [new SlotScoreDto(2, 50000)]);
        
        var result = await _sut.HandleAsync(command, CancellationToken.None);
        
        await _realtimeLeaderboardService.Received(1).AddScoreAsync(
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
        var now = DateTime.UtcNow;
        var gameSession = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", now);
        gameSession.AddPlayer(1, Guid.NewGuid(), "AAA");
        gameSession.AddPlayer(2, Guid.NewGuid(), "BBB");
        gameSession.Launch(now);

        _writeRepository.GetSessionWithMachineAsync(
                gameSession.SessionId, 
                Arg.Any<CancellationToken>())
            .Returns(gameSession);

        var scores = new List<SlotScoreDto>
        {
            new(1, 1500000),
            new(2, 850000)
        };
        var command = new SubmitScoreCommand(gameSession.SessionId, gameSession.DeviceId, scores);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SubmitScoreResult.Success>();
        gameSession.State.Should().Be(GameSessionState.Ended);
        await _realtimeLeaderboardService.Received(2).AddScoreAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), 
            Arg.Any<Guid>(), 
            Arg.Any<long>(), Arg.Any<CancellationToken>());
        
        await _lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(
            gameSession.SessionId, 1, 1500000, Arg.Any<CancellationToken>());
        await _lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(
            gameSession.SessionId, 2, 850000, Arg.Any<CancellationToken>());
        await _lobbyNotifier.Received(1).NotifySessionEndedAsync(
            gameSession.SessionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotStarted_ShouldReturnSessionNotStarted()
    {
        var session = new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "1.0", 
            "nonce",
            DateTime.UtcNow);

        _writeRepository.GetSessionWithMachineAsync(session.SessionId,
            Arg.Any<CancellationToken>())
            .Returns(session);
        
        session.AddPlayer(1, Guid.NewGuid(), "AAA");
        
        var command = new SubmitScoreCommand(session.SessionId,
            session.DeviceId, [new SlotScoreDto(1, 50000)]);
        
        var result = await _sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionNotStarted>();
    }
    

    
}
