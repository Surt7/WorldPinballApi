using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using WorldPinball.Infrastructure.Security.Authorization;

namespace WorldPinball.Tests.Infrastructure.Security.Authorization;

public sealed class ActiveMachineAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenMachineIsActive_ShouldSucceed()
    {
        // Arrange
        var handler = new ActiveMachineAuthorizationHandler();
        var requirement = new ActiveMachineRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("machine_status", "Active")
        ], "TestAuth"));

        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("Suspended")]
    [InlineData("Pending")]
    [InlineData(null)]
    public async Task HandleAsync_WhenMachineIsNotActive_ShouldNotSucceed(string? status)
    {
        // Arrange
        var handler = new ActiveMachineAuthorizationHandler();
        var requirement = new ActiveMachineRequirement();
        var claims = status is not null ? [new Claim("machine_status", status)] : Array.Empty<Claim>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
