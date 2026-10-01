using FluentAssertions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Domain.Entities;

public class StreamSubscriptionTests
{
    [Fact]
    public void Constructor_WhenArgumentsValid_ShouldInitializeSubscription()
    {
        // Arrange
        var subscriberId = Guid.NewGuid();
        var targetOrgId = Guid.NewGuid();
        var subscribedAtUtc = DateTime.UtcNow;

        // Act
        var subscription = new StreamSubscription(
            subscriberId,
            PrincipalType.Organization,
            targetOrgId,
            subscribedAtUtc,
            notificationsEnabled: true);

        // Assert
        subscription.SubscriptionId.Should().NotBeEmpty();
        subscription.SubscriberUserId.Should().Be(subscriberId);
        subscription.TargetType.Should().Be(PrincipalType.Organization);
        subscription.TargetPrincipalId.Should().Be(targetOrgId);
        subscription.SubscribedAtUtc.Should().Be(subscribedAtUtc);
        subscription.NotificationsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WhenSubscriberUserIdEmpty_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new StreamSubscription(
            Guid.Empty,
            PrincipalType.Organization,
            Guid.NewGuid(),
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("subscriberUserId");
    }

    [Fact]
    public void Constructor_WhenTargetPrincipalIdEmpty_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new StreamSubscription(
            Guid.NewGuid(),
            PrincipalType.Organization,
            Guid.Empty,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("targetPrincipalId");
    }

    [Fact]
    public void Constructor_WhenUserSubscribesToThemselves_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sameUserId = Guid.NewGuid();

        // Act
        var act = () => new StreamSubscription(
            sameUserId,
            PrincipalType.User,
            sameUserId,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A user cannot subscribe to themselves.");
    }

    [Fact]
    public void SetNotifications_WhenChanged_ShouldUpdateState()
    {
        // Arrange
        var subscription = new StreamSubscription(
            Guid.NewGuid(),
            PrincipalType.Organization,
            Guid.NewGuid(),
            DateTime.UtcNow,
            notificationsEnabled: true);

        // Act
        subscription.SetNotifications(false);

        // Assert
        subscription.NotificationsEnabled.Should().BeFalse();
    }
}
