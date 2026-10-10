using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.RecordBallScore;
using WorldPinball.Application.Features.GameSessions.Public.RecordBallScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class RecordBallScoreHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly ILobbyNotificationService _lobbyNotifier = Substitute.For<ILobbyNotificationService>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 30, 10, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<RecordBallScoreHandler> _logger = NullLogger<RecordBallScoreHandler>.Instance;
    private readonly RecordBallScoreHandler _sut;

    public RecordBallScoreHandlerTests()
    {
        _sut = new RecordBallScoreHandler(_writeRepo, _lobbyNotifier, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);

        var command = new RecordBallScoreCommand(sessionId, Guid.NewGuid(), 1, 1, 50_000);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RecordBallScoreResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenDeviceIdMismatch_ShouldReturnDeviceMismatch()
    {
        // Arrange
        var expectedDeviceId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(), expectedDeviceId, Guid.NewGuid(), "1.0", "nonce", _timeProvider.GetUtcNow().UtcDateTime);
        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var wrongDeviceId = Guid.NewGuid();
        var command = new RecordBallScoreCommand(session.SessionId, wrongDeviceId, 1, 1, 50_000);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RecordBallScoreResult.DeviceMisMatch>()
            .Which.ActualDeviceId.Should().Be(wrongDeviceId);
    }
    
    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldRecordBallScoreUpdateSessionAndNotify()
    {
        // Arrange: create session, add player, then launch
        var deviceId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(), deviceId, Guid.NewGuid(), "1.0", "nonce", _timeProvider.GetUtcNow().UtcDateTime);
        session.AddPlayer(slotNumber: 1, userId: Guid.NewGuid());
        session.Launch(_timeProvider.GetUtcNow().UtcDateTime);
        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);

        var command = new RecordBallScoreCommand(session.SessionId, deviceId, 1, 2, 850_000);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RecordBallScoreResult.Success>();
        await _writeRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
        await _lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(session.SessionId, 1, 850_000, Arg.Any<CancellationToken>());
    }
}