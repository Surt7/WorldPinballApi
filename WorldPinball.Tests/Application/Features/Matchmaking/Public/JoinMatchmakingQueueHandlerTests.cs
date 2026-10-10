using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin.Provision.Dtos;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Enums;


namespace WorldPinball.Tests.Application.Features.Matchmaking.Public;

public class JoinMatchmakingQueueHandlerTests
{
    private readonly IMachineReadRepository _machineReadRepo = Substitute.For<IMachineReadRepository>();
    private readonly IMatchmakingQueueService _queueService = Substitute.For<IMatchmakingQueueService>();
    private readonly JoinMatchmakingQueueHandler _sut;
    private readonly FakeLogger<JoinMatchmakingQueueHandler> _logger = new();
    
    public JoinMatchmakingQueueHandlerTests()
    {
        _sut = new JoinMatchmakingQueueHandler(_machineReadRepo, _queueService, _logger);
    }
 
    [Fact]
    public async Task HandleAsync_WhenMachineDoesNotExist_ShouldReturnMachineNotFound()
    {
        
        
        // Arrange
        var machineId = Guid.NewGuid();
        _machineReadRepo.GetMachinesWithId(machineId, Arg.Any<CancellationToken>())
            .Returns((MachineRowDto?)null);

        var command = new JoinMatchmakingQueueCommand(machineId, Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<JoinMatchmakingQueueResult.MachineNotFound>();
        await _queueService.DidNotReceiveWithAnyArgs().EnqueueAsync(default!, default);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenMachineExist_ShouldReturnSuccess()
    {
        
        
        // Arrange
        var machineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var machineDto = new MachineRowDto(
            machineId,
            "Machine-42",
            "Godzilla",
            MachineStatus.Active,
            "Ok let's go",
            DateTime.UtcNow,
            null,
            SerialNumber: "WP-AR-12Z-ZZS"
        );
        
        _machineReadRepo.GetMachinesWithId(machineId, Arg.Any<CancellationToken>())
            .Returns(machineDto);

        var command = new JoinMatchmakingQueueCommand(machineId,playerId );

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<JoinMatchmakingQueueResult.Success>();
        await _queueService.Received(1).EnqueueAsync(machineId.ToString(),playerId, Arg.Any<CancellationToken>());
    }
}