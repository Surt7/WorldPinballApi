using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Queries;
using WorldPinball.Application.Interfaces.Repositories;
namespace WorldPinball.Tests.Application.Features.Leaderboard.Admin.Dashboard;

public class GetMachineDashboardHandlerTests
{
    private readonly IMachineDashboardReadRepository _readRepo = Substitute.For<IMachineDashboardReadRepository>();
    private readonly NullLogger<GetMachineDashboardHandler> _logger = NullLogger<GetMachineDashboardHandler>.Instance;

    
    [Fact]
    public async Task HandleAsync_WhenMachineExistsShouldReturnSuccess()
    {

        var machineId = Guid.NewGuid();
        var stats = new MachineDashboardDto(machineId, "WP-123", 10, 5000000, 2500000, 4, DateTime.UtcNow);
        
        _readRepo.GetMachineStatsAsync(machineId, Arg.Any<CancellationToken>()).Returns(stats);
        var sut = new GetMachineDashboardHandler(_readRepo, _logger);
        var query = new GetMachineDashboardQuery(machineId);

        
        var result = await sut.HandleAsync(query);
        
        result.Should().BeOfType<GetMachineDashboardResult.Success>().Which.Stats.Should().Be(stats);
    }
    
    
      
    [Fact]
    public async Task HandleAsync_WhenMachineNotFound_ShouldReturnNotFound()
    {

        var machineId = Guid.NewGuid();
       
        
        _readRepo.GetMachineStatsAsync(machineId, Arg.Any<CancellationToken>()).Returns((MachineDashboardDto?)null);
        var sut = new GetMachineDashboardHandler(_readRepo, _logger);
        var query = new GetMachineDashboardQuery(machineId);

        
        var result = await sut.HandleAsync(query);
        
        result.Should().BeOfType<GetMachineDashboardResult.NotFound>();
        
        
        
    }
    
    [Fact]
    public async Task HandleAsync_WhenMachineIdIsEmpty_ShouldReturnNotFound()
    {
        // Arrange
        var sut = new GetMachineDashboardHandler(_readRepo, _logger);
        var query = new GetMachineDashboardQuery(Guid.Empty);
        // Act
        var result = await sut.HandleAsync(query);
        // Assert
        result.Should().BeOfType<GetMachineDashboardResult.NotFound>();
        await _readRepo.DidNotReceive().GetMachineStatsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
    
    
    
}