using FluentAssertions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Domain.Entities;

public class StreamAccessGrantTests
{
    [Fact]
    public void Constructor_WhenArgumentsValid_ShouldInitializeActiveGrant()
    {
        // Arrange
        var principalId = Guid.NewGuid();
        var displayName = "Stern Pinball Official";
        var createdAtUtc = DateTime.UtcNow;
        var expiresAtUtc = createdAtUtc.AddYears(1);
        var adminId = Guid.NewGuid();

        // Act
        var grant = new StreamAccessGrant(
            PrincipalType.Organization,
            principalId,
            displayName,
            GrantOrigin.Whitelist,
            createdAtUtc,
            expiresAtUtc,
            adminId);

        // Assert
        grant.GrantId.Should().NotBeEmpty();
        grant.PrincipalType.Should().Be(PrincipalType.Organization);
        grant.PrincipalId.Should().Be(principalId);
        grant.DisplayName.Should().Be(displayName);
        grant.Origin.Should().Be(GrantOrigin.Whitelist);
        grant.Status.Should().Be(GrantStatus.Active);
        grant.CreatedAtUtc.Should().Be(createdAtUtc);
        grant.ExpiresAtUtc.Should().Be(expiresAtUtc);
        grant.GrantedByUserId.Should().Be(adminId);
        grant.RevokedAtUtc.Should().BeNull();
        grant.RevocationReason.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenPrincipalIdEmpty_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new StreamAccessGrant(
            PrincipalType.User,
            Guid.Empty,
            "Player One",
            GrantOrigin.ByApproval,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("principalId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WhenDisplayNameEmptyOrNull_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange & Act
        var act = () => new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            invalidName!,
            GrantOrigin.ByApproval,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

    [Fact]
    public void Constructor_WhenExpiresAtInPast_ShouldThrowArgumentException()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var act = () => new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.ByApproval,
            now,
            expiresAtUtc: now.AddMinutes(-5));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("expiresAtUtc");
    }

    [Fact]
    public void Revoke_WhenActiveAndReasonProvided_ShouldSetRevokedStatus()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        var revokedAt = DateTime.UtcNow.AddDays(1);
        var reason = "Partner agreement terminated.";

        // Act
        grant.Revoke(reason, revokedAt);

        // Assert
        grant.Status.Should().Be(GrantStatus.Revoked);
        grant.RevokedAtUtc.Should().Be(revokedAt);
        grant.RevocationReason.Should().Be(reason);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        grant.Revoke("First revocation", DateTime.UtcNow);

        // Act
        var act = () => grant.Revoke("Second revocation", DateTime.UtcNow);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Grant is already revoked.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Revoke_WhenReasonEmptyOrNull_ShouldThrowArgumentException(string? invalidReason)
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        // Act
        var act = () => grant.Revoke(invalidReason!, DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("reason");
    }

    [Fact]
    public void Reactivate_WhenRevoked_ShouldResetToActive()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        grant.Revoke("Violation of TOS", DateTime.UtcNow);

        // Act
        grant.Reactivate();

        // Assert
        grant.Status.Should().Be(GrantStatus.Active);
        grant.RevokedAtUtc.Should().BeNull();
        grant.RevocationReason.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenActiveAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            now,
            expiresAtUtc: now.AddDays(7));

        // Act & Assert
        grant.IsActive(now).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var expiresAt = createdAt.AddDays(5);
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            createdAt,
            expiresAtUtc: expiresAt);

        // Act & Assert
        grant.IsActive(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "Player One",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        grant.Revoke("Revocation", DateTime.UtcNow);

        // Act & Assert
        grant.IsActive(DateTime.UtcNow).Should().BeFalse();
    }
}
