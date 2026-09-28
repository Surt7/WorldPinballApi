using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class KickPlayerHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly NullLogger<KickPlayerHandler> _logger = NullLogger<KickPlayerHandler>.Instance;

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public async Task HandleAsync_WhenSlotInvalid_ShouldReturnInvalidSlot(int slotNumber)
    {
        // Arrange
        var sut = new KickPlayerHandler(_writeRepo, _logger);
        var command = new KickPlayerCommand(Guid.NewGuid(), slotNumber);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<KickPlayerResult.InvalidSlot>()
            .Which.SlotNumber.Should().Be(slotNumber);
        await _writeRepo.DidNotReceive().GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSessionDoesntExist_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);

        var sut = new KickPlayerHandler(_writeRepo, _logger);
        var command = new KickPlayerCommand(sessionId, SlotNumber: 1);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<KickPlayerResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionAlreadyStarted_ShouldReturnCannotRemovePlayerActiveSession()
    {
        // Arrange
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(1, Guid.NewGuid(), "AAA", requiresRegisteredUser: false);
        session.Launch(DateTime.UtcNow);

        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new KickPlayerHandler(_writeRepo, _logger);
        var command = new KickPlayerCommand(session.SessionId, SlotNumber: 1);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<KickPlayerResult.CannotRemovePlayerActiveSession>()
            .Which.SessionId.Should().Be(session.SessionId);
        await _writeRepo.DidNotReceive().UpdateSessionAsync(Arg.Any<GameSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPlayerNotInSlot_ShouldReturnPlayerNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(1, Guid.NewGuid(), "AAA", requiresRegisteredUser: false);

        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new KickPlayerHandler(_writeRepo, _logger);
        var command = new KickPlayerCommand(sessionId, SlotNumber: 2);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<KickPlayerResult.PlayerNotFound>()
            .Which.SlotNumber.Should().Be(2);
        await _writeRepo.DidNotReceive().UpdateSessionAsync(Arg.Any<GameSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPlayerInSlotAndSessionNotStarted_ShouldRemovePlayerAndUpdateSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(1, Guid.NewGuid(), "AAA", requiresRegisteredUser: false);
        session.AddPlayer(2, Guid.NewGuid(), "BBB", requiresRegisteredUser: false);

        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new KickPlayerHandler(_writeRepo, _logger);
        var command = new KickPlayerCommand(sessionId, SlotNumber: 2, Reason: "Afk");

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<KickPlayerResult.Success>()
            .Which.SlotNumber.Should().Be(2);
        session.Players.Should().HaveCount(1);
        session.Players.Should().NotContain(p => p.SlotNumber == 2);
        await _writeRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }
}
