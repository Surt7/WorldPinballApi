using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure.Security.Authorization;

namespace WorldPinball.Tests.Infrastructure.Security.Authorization;

public class GameSessionParticipantAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsParticipant_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ]));
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(slotNumber: 1, userId: userId);

        var requirement = new GameSessionParticipantRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, session);
        var sut = new GameSessionParticipantAuthorizationHandler();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
    
    
    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNotParticipant_ShouldNotSucceed()
    {
        // Arrange
        var strangerId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, strangerId.ToString())
        ]));
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        session.AddPlayer(slotNumber: 1, userId: Guid.NewGuid()); // Alice is in slot 1, not Bob!

        var requirement = new GameSessionParticipantRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, session);
        var sut = new GameSessionParticipantAuthorizationHandler();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
    
    
    [Fact]
    public async Task HandleRequirementAsync_WhenCallerIsMachine_ShouldSucceed()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, machineId.ToString())
        ]));
        var session = new GameSession(machineId, Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", DateTime.UtcNow);
        var requirement = new GameSessionParticipantRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, session);
        var sut = new GameSessionParticipantAuthorizationHandler();
        // Act
        await sut.HandleAsync(context);
        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}