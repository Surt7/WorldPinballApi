using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class LaunchSessionHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 26, 12, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<LaunchSessionHandler> _logger = NullLogger<LaunchSessionHandler>.Instance;

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);

        var sut = new LaunchSessionHandler(_writeRepo, _timeProvider, _logger);
        var command = new LaunchSessionCommand(sessionId);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<LaunchSessionResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionInCreatedState_ShouldStartSessionAndSave()
    {
        // Arrange
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);
        session.State.Should().Be(GameSessionState.Created);

        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new LaunchSessionHandler(_writeRepo, _timeProvider, _logger);
        var command = new LaunchSessionCommand(session.SessionId);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<LaunchSessionResult.Success>()
            .Which.SessionId.Should().Be(session.SessionId);

        session.State.Should().Be(GameSessionState.Started);
        await _writeRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSessionAlreadyEnded_ShouldReturnCannotStartSession()
    {
        // Arrange
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);
        session.Start(DateTime.UtcNow);
        session.End(DateTime.UtcNow.AddMinutes(5));

        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new LaunchSessionHandler(_writeRepo, _timeProvider, _logger);
        var command = new LaunchSessionCommand(session.SessionId);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<LaunchSessionResult.CannotStartSession>();
        await _writeRepo.DidNotReceive().UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }
}
