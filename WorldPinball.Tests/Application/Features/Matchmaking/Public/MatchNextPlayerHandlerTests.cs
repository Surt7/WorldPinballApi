using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer;
using WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.Matchmaking.Public;

public class MatchNextPlayerHandlerTests
{
    private readonly IGameSessionWriteRepository _sessionWriteRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly IMatchmakingQueueService _queueService = Substitute.For<IMatchmakingQueueService>();
    private readonly ILobbyNotificationService _lobbyNotifier = Substitute.For<ILobbyNotificationService>();
    private readonly NullLogger<MatchNextPlayerHandler> _logger = NullLogger<MatchNextPlayerHandler>.Instance;
    private readonly MatchNextPlayerHandler _sut;

    public MatchNextPlayerHandlerTests()
    {
        _sut = new MatchNextPlayerHandler(_sessionWriteRepo, _queueService, _lobbyNotifier, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _sessionWriteRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);
        var command = new MatchNextPlayerCommand(Guid.NewGuid(), sessionId);
        // Act
        var result = await _sut.HandleAsync(command);
        // Assert
        result.Should().BeOfType<MatchNextPlayerResult.SessionNotFound>();
        await _queueService.DidNotReceiveWithAnyArgs().DequeueNextAsync(default!, default);
    }


    [Fact]
    public async Task HandleAsync_WhenQueueIsEmpty_ShouldReturnNoPlayerInQueue()
    {
        //Arrange
        var machineId = Guid.NewGuid();
        var session = new GameSession(machineId, Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        _sessionWriteRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);

        _queueService.DequeueNextAsync(machineId.ToString(), Arg.Any<CancellationToken>()).Returns((Guid?)null);
        // Act
        var result = await _sut.HandleAsync(new MatchNextPlayerCommand(machineId, session.SessionId));

        // Assert
        result.Should().BeOfType<MatchNextPlayerResult.NoPlayerInQueue>();
        await _sessionWriteRepo.DidNotReceive().UpdateSessionAsync(default!, default);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenSessionIsFull_ShouldReturnSessionFull()
    {
        //Arrange
        var machineId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();
        var playerId3 = Guid.NewGuid();
        var playerId4 = Guid.NewGuid();
        var session = new GameSession(machineId, Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        _sessionWriteRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);

        session.AddPlayer(1,playerId1);
        session.AddPlayer(2,playerId2);
        session.AddPlayer(3,playerId3);
        session.AddPlayer(4,playerId4);
        session.Players.Count.Should().Be(4);
        _queueService.DequeueNextAsync(machineId.ToString(), Arg.Any<CancellationToken>()).Returns(machineId);
        // Act
        var result = await _sut.HandleAsync(new MatchNextPlayerCommand(machineId, session.SessionId));

        // Assert
        result.Should().BeOfType<MatchNextPlayerResult.SessionFull>();
        await _sessionWriteRepo.DidNotReceive().UpdateSessionAsync(default!, default);
    }
    
    
    
    [Fact]
    public async Task HandleAsync_QueueIsUpdated_ShouldReturnSuccess()
    {
        //Arrange
        var machineId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();

        var session = new GameSession(machineId, Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        _sessionWriteRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);

        session.AddPlayer(1,playerId1);
      
        session.Players.Count.Should().NotBe(4);
        _queueService.DequeueNextAsync(machineId.ToString(), Arg.Any<CancellationToken>()).Returns(machineId);
        // Act
        var result = await _sut.HandleAsync(new MatchNextPlayerCommand(machineId, session.SessionId));

        // Assert
        result.Should().BeOfType<MatchNextPlayerResult.Success>();
        await _sessionWriteRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMatchFoundState_ShouldNotifyMatchFound()
    {
        // Arrange: player 1 is searching for opponents
        var machineId = Guid.NewGuid();
        var session = new GameSession(machineId, Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(1, Guid.NewGuid());
        session.SearchPlayers();

        _sessionWriteRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);
        var player2Id = Guid.NewGuid();
        _queueService.DequeueNextAsync(machineId.ToString(), Arg.Any<CancellationToken>()).Returns(player2Id);

        // Act
        var result = await _sut.HandleAsync(new MatchNextPlayerCommand(machineId, session.SessionId));

        // Assert
        result.Should().BeOfType<MatchNextPlayerResult.Success>();
        var success = (MatchNextPlayerResult.Success)result;
        success.MatchFound.Should().BeTrue();
        session.State.Should().Be(GameSessionState.MatchFound);
        await _lobbyNotifier.Received(1).NotifyMatchFoundAsync(session.SessionId, session.MachineId, Arg.Any<CancellationToken>());
    }
}