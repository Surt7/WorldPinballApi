using System.Security.Claims;
using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin.Provision.Dtos;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Security.Authentification;

namespace WorldPinball.Tests.Infrastructure.Security.Authentification;

public class MachineClaimsTransformationTests
{

    [Fact]
    public async Task TransformAsync_WhenClaimAlreadyExists_ShouldReturnWithoutCallingCacheOrDb()
    {
        // Arrange
        var cache = Substitute.For<IMachineStatusCacheService>();
        var repository = Substitute.For<IMachineReadRepository>();
        
        var sut = new MachineClaimsTransformation(cache, repository);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("machine_status", "Active")
        ], "TestAuth"));

        // Act
        var result = await sut.TransformAsync(principal);
        
        // Assert
        result.FindFirst("machine_status")!.Value.Should().Be("Active");
        await cache.DidNotReceiveWithAnyArgs().GetStatusAsync(Guid.Empty);
        await repository.DidNotReceiveWithAnyArgs().GetMachinesWithId(Guid.Empty);

    }
    
    [Fact]
    public async Task TransformAsync_WhenStatusIsInRedisCache_ShouldAddClaimWithoutCallingDb()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var cache = Substitute.For<IMachineStatusCacheService>();
        var repository = Substitute.For<IMachineReadRepository>();
        cache.GetStatusAsync(machineId, Arg.Any<CancellationToken>())
            .Returns("Active");
        var sut = new MachineClaimsTransformation(cache, repository);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, machineId.ToString())
        ], "TestAuth"));
        
        
        // Act
        var result = await sut.TransformAsync(principal);
        
        
        // Assert
        result.FindFirst("machine_status")?.Value.Should().Be("Active");
        await repository.DidNotReceiveWithAnyArgs().GetMachinesWithId(Guid.Empty);
    }
    
    
    [Fact]
    public async Task TransformAsync_WhenNotInRedis_ShouldFetchFromDbAndWarmupRedis()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var cache = Substitute.For<IMachineStatusCacheService>();
        var repository = Substitute.For<IMachineReadRepository>();
        
        cache.GetStatusAsync(machineId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var machineRow = new MachineRowDto(
            machineId, "Godzilla", "Godzilla LE", MachineStatus.Active,
            null, DateTime.UtcNow, null, "SN-12345");

        repository.GetMachinesWithId(machineId, Arg.Any<CancellationToken>())
            .Returns(machineRow);

        var sut = new MachineClaimsTransformation(cache, repository);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, machineId.ToString())
        ], "TestAuth"));

        // Act
        var result = await sut.TransformAsync(principal);

        // Assert
        result.FindFirst("machine_status")?.Value.Should().Be("Active");
        await cache.Received(1).SetStatusAsync(
            machineId, "Active", TimeSpan.FromMinutes(5), Arg.Any<CancellationToken>());
    }
}