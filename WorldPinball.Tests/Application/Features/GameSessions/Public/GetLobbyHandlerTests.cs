using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Queries;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class GetLobbyHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly NullLogger<GetLobbyHandler> _logger = NullLogger<GetLobbyHandler>.Instance;

    [Fact]
    public async Task HandleAsync_WhenSessionDoesntExist_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _writeRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);

        var sut = new GetLobbyHandler(_writeRepo, _logger);
        var query = new GetLobbyQuery(sessionId);

        // Act
        var result = await sut.HandleAsync(query);

        // Assert
        result.Should().BeOfType<GetLobbyResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionHasNoPlayers_ShouldReturnSuccessWithFourEmptySlots()
    {
        // Arrange
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);

        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new GetLobbyHandler(_writeRepo, _logger);
        var query = new GetLobbyQuery(session.SessionId);

        // Act
        var result = await sut.HandleAsync(query);

        // Assert
        var success = result.Should().BeOfType<GetLobbyResult.Success>().Subject;
        success.Lobby.SessionId.Should().Be(session.SessionId);
        success.Lobby.SessionState.Should().Be("Created");
        success.Lobby.Slots.Should().HaveCount(4);

        for (int i = 1; i <= 4; i++)
        {
            var slot = success.Lobby.Slots.First(s => s.SlotNumber == i);
            slot.IsOccupied.Should().BeFalse();
            slot.UserId.Should().BeNull();
            slot.GuestInitials.Should().BeNull();
        }
    }

    [Fact]
    public async Task HandleAsync_WhenSessionHasRegisteredAndGuestPlayers_ShouldMapSlotsCorrectly()
    {
        // Arrange
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0.0", "nonce", DateTime.UtcNow);

        var user1Id = Guid.NewGuid();
        session.AddPlayer(slotNumber: 1, userId: user1Id, guestInitials: null, requiresRegisteredUser: true);
        session.AddPlayer(slotNumber: 2, userId: null, guestInitials: "BOB", requiresRegisteredUser: false);

        _writeRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var sut = new GetLobbyHandler(_writeRepo, _logger);
        var query = new GetLobbyQuery(session.SessionId);

        // Act
        var result = await sut.HandleAsync(query);

        // Assert
        var success = result.Should().BeOfType<GetLobbyResult.Success>().Subject;
        success.Lobby.SessionId.Should().Be(session.SessionId);
        success.Lobby.Slots.Should().HaveCount(4);

        // Slot 1: Registered User
        var slot1 = success.Lobby.Slots.First(s => s.SlotNumber == 1);
        slot1.IsOccupied.Should().BeTrue();
        slot1.UserId.Should().Be(user1Id);
        slot1.GuestInitials.Should().BeNull();

        // Slot 2: Guest Player
        var slot2 = success.Lobby.Slots.First(s => s.SlotNumber == 2);
        slot2.IsOccupied.Should().BeTrue();
        slot2.UserId.Should().BeNull();
        slot2.GuestInitials.Should().Be("BOB");

        // Slots 3 & 4: Empty
        var slot3 = success.Lobby.Slots.First(s => s.SlotNumber == 3);
        slot3.IsOccupied.Should().BeFalse();

        var slot4 = success.Lobby.Slots.First(s => s.SlotNumber == 4);
        slot4.IsOccupied.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var sut = new GetLobbyHandler(_writeRepo, _logger);
        var query = new GetLobbyQuery(sessionId);

        // Act
        await sut.HandleAsync(query, token);

        // Assert
        await _writeRepo.Received(1).GetSessionWithMachineAsync(sessionId, token);
    }
}
