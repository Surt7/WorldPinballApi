using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Features.Roles;
using WorldPinball.Application.Features.Roles.Admin.RevokePermission;
using WorldPinball.Application.Features.Roles.Admin.RevokePermission.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using Xunit;

namespace WorldPinball.Tests.Application.Features.Roles;

public class RevokePermissionFromRoleHandlerTests
{
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

    [Fact]
    public async Task HandleAsync_WhenNotFound_ShouldReturnNotFoundAndNotRemove()
    {
        // Arrange
        var repo = Substitute.For<IRolePermissionRepository>();
        var reloader = Substitute.For<IRolePermissionReloader>();
        repo.ExistsAsync("Technician", "MachinesProvision").Returns(false);
        var sut = new RevokePermissionFromRoleHandler(repo, reloader);

        // Act
        var result = await sut.HandleAsync(new RevokePermissionFromRoleCommand("Technician", "MachinesProvision"));

        // Assert
        result.Should().BeOfType<RevokePermissionFromRoleResult.NotFound>();
        await repo.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await reloader.DidNotReceive().ReloadFromDatabaseAsync(Arg.Any<CancellationToken>());
    }
}
