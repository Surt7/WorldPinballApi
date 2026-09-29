using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.AbortSession;
using WorldPinball.Application.Features.GameSessions.Public.AbortSession.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class AbortSessionHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly ILobbyNotificationService _lobbyNotifier = Substitute.For<ILobbyNotificationService>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 29, 14, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<AbortSessionHandler> _logger = NullLogger<AbortSessionHandler>.Instance;
    private readonly AbortSessionHandler _sut;

    public AbortSessionHandlerTests()
    {
        _sut = new AbortSessionHandler(_writeRepo, _lobbyNotifier, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);

        // Act
        var result = await _sut.HandleAsync(new AbortSessionCommand(sessionId));

        // Assert
        result.Should().BeOfType<AbortSessionResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenSessionIsActive_ShouldAbortSaveAndNotify()
    {
        // Arrange: active started session
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", _timeProvider.GetUtcNow().UtcDateTime);
        session.Launch(_timeProvider.GetUtcNow().UtcDateTime);
        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _sut.HandleAsync(new AbortSessionCommand(session.SessionId));

        // Assert
        result.Should().BeOfType<AbortSessionResult.Success>()
            .Which.SessionId.Should().Be(session.SessionId);
        session.State.Should().Be(GameSessionState.Aborted);
        await _writeRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
        await _lobbyNotifier.Received(1).NotifySessionAbortedAsync(session.SessionId, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task HandleAsync_WhenSessionAlreadyEnded_ShouldReturnCannotAbortSession()
    {
        // Arrange: session already ended
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", now);
        session.Launch(now);
        session.End(now.AddMinutes(5));
        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _sut.HandleAsync(new AbortSessionCommand(session.SessionId));

        // Assert
        result.Should().BeOfType<AbortSessionResult.CannotAbortSession>();
        await _writeRepo.DidNotReceive().UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }
}