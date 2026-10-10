using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using WorldPinball.Infrastructure.Security.Authorization;
using Xunit;

namespace WorldPinball.Tests.Infrastructure.Security.Authorization;

public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasRoleWithPermission_ShouldSucceed()
    {
        // Arrange
        var store = Substitute.For<IRolePermissionStore>();
        store.HasPermission(Arg.Any<IEnumerable<string>>(), "MachinesProvision").Returns(true);
        var sut = new PermissionAuthorizationHandler(store);

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Technician")]));
        var req = new PermissionRequirement("MachinesProvision");
        var context = new AuthorizationHandlerContext([req], user, null);

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserLacksPermission_ShouldNotSucceed()
    {
        // Arrange
        var store = Substitute.For<IRolePermissionStore>();
        store.HasPermission(Arg.Any<IEnumerable<string>>(), "MachinesProvision").Returns(false);
        var sut = new PermissionAuthorizationHandler(store);

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Player")]));
        var req = new PermissionRequirement("MachinesProvision");
        var context = new AuthorizationHandlerContext([req], user, null);

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
