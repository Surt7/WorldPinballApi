using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Features.Roles;
using WorldPinball.Application.Features.Roles.Admin.GrantPermission;
using WorldPinball.Application.Features.Roles.Admin.GrantPermission.Commands;
using WorldPinball.Application.Features.Roles.Admin.RevokePermission;
using WorldPinball.Application.Features.Roles.Admin.RevokePermission.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Tests.Application.Features.Roles;

public class GrantPermissionToRoleHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenValidAndNotExisting_ShouldPersistReloadAndReturnSuccess()
    {
        // Arrange
        var repo = Substitute.For<IRolePermissionRepository>();
        var reloader = Substitute.For<IRolePermissionReloader>();
        repo.ExistsAsync("Technician", "MachinesProvision").Returns(false);
        var sut = new GrantPermissionToRoleHandler(repo, reloader);

        // Act
        var result = await sut.HandleAsync(new GrantPermissionToRoleCommand("Technician",
            "MachinesProvision"));

        // Assert
        result.Should().BeOfType<GrantPermissionToRoleResult.Success>();
        
        await repo.Received(1).AddAsync("Technician", "MachinesProvision", 
            Arg.Any<CancellationToken>());
        
        await reloader.Received(1).ReloadFromDatabaseAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task HandleAsync_WhenAlreadyExists_ShouldReturnAlreadyExistsAndNotPersist()
    {
        // Arrange
        var repo = Substitute.For<IRolePermissionRepository>();
        var reloader = Substitute.For<IRolePermissionReloader>();
        
        repo.ExistsAsync("Technician", "MachinesProvision").Returns(true);
        
        var sut = new GrantPermissionToRoleHandler(repo, reloader);
        
        // Act
        var result = await sut.HandleAsync(new GrantPermissionToRoleCommand("Technician", 
            "MachinesProvision"));
        
        
        // Assert
        result.Should().BeOfType<GrantPermissionToRoleResult.AlreadyExists>();
        await repo.DidNotReceive().AddAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await reloader.DidNotReceive().ReloadFromDatabaseAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task HandleAsync_WhenExisting_ShouldRemoveReloadAndReturnSuccess()
    {
        // Arrange
        var repo = Substitute.For<IRolePermissionRepository>();
        var reloader = Substitute.For<IRolePermissionReloader>();
        repo.ExistsAsync("Technician", "MachinesProvision").Returns(true);
        var sut = new RevokePermissionFromRoleHandler(repo, reloader);

        // Act
        var result = await sut.HandleAsync(new RevokePermissionFromRoleCommand("Technician", "MachinesProvision"));

        // Assert
        result.Should().BeOfType<RevokePermissionFromRoleResult.Success>();
        await repo.Received(1).RemoveAsync("Technician", "MachinesProvision", Arg.Any<CancellationToken>());
        await reloader.Received(1).ReloadFromDatabaseAsync(Arg.Any<CancellationToken>());
    }
}