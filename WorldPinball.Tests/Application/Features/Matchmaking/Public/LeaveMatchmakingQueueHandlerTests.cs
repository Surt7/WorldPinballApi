using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Tests.Application.Features.Matchmaking.Public;

public class LeaveMatchmakingQueueHandlerTests
{
    private readonly IMatchmakingQueueService _queueService = Substitute.For<IMatchmakingQueueService>();
    private readonly NullLogger<LeaveMatchmakingQueueHandler> _logger = NullLogger<LeaveMatchmakingQueueHandler>.Instance;
    private readonly LeaveMatchmakingQueueHandler _sut;

    public LeaveMatchmakingQueueHandlerTests()
    {
        _sut = new LeaveMatchmakingQueueHandler(_queueService, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenPlayerInQueue_ShouldRemoveAndReturnSuccess()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _queueService.RemoveAsync(machineId.ToString(), playerId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new LeaveMatchmakingQueueCommand(machineId, playerId);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<LeaveMatchmakingQueueResult.Success>();
        await _queueService.Received(1).RemoveAsync(machineId.ToString(), playerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPlayerNotInQueue_ShouldReturnPlayerNotInQueue()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _queueService.RemoveAsync(machineId.ToString(), playerId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveMatchmakingQueueCommand(machineId, playerId);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<LeaveMatchmakingQueueResult.PlayerNotInQueue>();
    }
}
