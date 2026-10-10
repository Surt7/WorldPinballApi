using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Contracts.Matchmaking;
using WorldPinball.Api.Controllers.Public;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;

namespace WorldPinball.Tests.Controller;

public class MatchmakingControllerTests
{
    private readonly IJoinMatchmakingQueueHandler _joinHandler = Substitute.For<IJoinMatchmakingQueueHandler>();
    private readonly ILeaveMatchmakingQueueHandler _leaveHandler = Substitute.For<ILeaveMatchmakingQueueHandler>();
    private readonly MatchmakingController _sut;

    public MatchmakingControllerTests()
    {
        _sut = new MatchmakingController(_joinHandler, _leaveHandler);
    }

    [Fact]
    public async Task Join_WhenHandlerReturnsSuccess_ShouldReturnOk()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _joinHandler.HandleAsync(
            Arg.Is<JoinMatchmakingQueueCommand>(c => c.MachineId == machineId && c.PlayerId == playerId),
            Arg.Any<CancellationToken>())
            .Returns(new JoinMatchmakingQueueResult.Success());

        var request = new JoinMatchmakingQueueRequest(machineId, playerId);

        // Act
        var result = await _sut.Join(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Join_WhenHandlerReturnsMachineNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _joinHandler.HandleAsync(
            Arg.Is<JoinMatchmakingQueueCommand>(c => c.MachineId == machineId && c.PlayerId == playerId),
            Arg.Any<CancellationToken>())
            .Returns(new JoinMatchmakingQueueResult.MachineNotFound(machineId));

        var request = new JoinMatchmakingQueueRequest(machineId, playerId);

        // Act
        var result = await _sut.Join(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Leave_WhenHandlerReturnsSuccess_ShouldReturnOk()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _leaveHandler.HandleAsync(
            Arg.Is<LeaveMatchmakingQueueCommand>(c => c.MachineId == machineId && c.PlayerId == playerId),
            Arg.Any<CancellationToken>())
            .Returns(new LeaveMatchmakingQueueResult.Success());

        var request = new LeaveMatchmakingQueueRequest(machineId, playerId);

        // Act
        var result = await _sut.Leave(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Leave_WhenHandlerReturnsPlayerNotInQueue_ShouldReturnNotFound()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _leaveHandler.HandleAsync(
            Arg.Is<LeaveMatchmakingQueueCommand>(c => c.MachineId == machineId && c.PlayerId == playerId),
            Arg.Any<CancellationToken>())
            .Returns(new LeaveMatchmakingQueueResult.PlayerNotInQueue());

        var request = new LeaveMatchmakingQueueRequest(machineId, playerId);

        // Act
        var result = await _sut.Leave(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
