using FluentAssertions;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Domain.Entities;

public class StreamingPartnerTests
{
    [Fact]
    public void Constructor_WhenArgumentsAreValid_ShouldInitializeActivePartner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organization = "IFPA World Championship";
        var createdAtUtc = DateTime.UtcNow;

        // Act
        var partner = new StreamingPartner(userId, organization, createdAtUtc);

        // Assert
        partner.PartnerId.Should().NotBeEmpty();
        partner.UserId.Should().Be(userId);
        partner.OrganizationName.Should().Be(organization);
        partner.IsActive.Should().BeTrue();
        partner.CreatedAtUtc.Should().Be(createdAtUtc);
        partner.RevokedAtUtc.Should().BeNull();
    }
    
    [Fact]
    public void Revoke_WhenPartnerIsActive_ShouldDeactivateAndSetRevokedAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var partner = new StreamingPartner(Guid.NewGuid(), "Stern Pro Circuit", createdAt);
        var revokedAt = createdAt.AddDays(10);

        // Act
        partner.Revoke(revokedAt);

        // Assert
        partner.IsActive.Should().BeFalse();
        partner.RevokedAtUtc.Should().Be(revokedAt);
    }
    
    [Fact]
    public void Reactivate_WhenPartnerWasRevoked_ShouldReactivateAndClearRevokedAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var partner = new StreamingPartner(Guid.NewGuid(), "Pinball Expo", createdAt);
        partner.Revoke(createdAt.AddDays(5));

        // Act
        partner.Reactivate();

        // Assert
        partner.IsActive.Should().BeTrue();
        partner.RevokedAtUtc.Should().BeNull();
    }
    
    
    [Fact]
    public void Constructor_WhenValid_ShouldInitializeInPendingStatus()
    {
        
        // Arrange & Act
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var request = new StreamingAccessRequest(sessionId, userId, createdAt);
        
        
        // Assert
        request.RequestId.Should().NotBeEmpty();
        request.SessionId.Should().Be(sessionId);
        request.RequestedByUserId.Should().Be(userId);
        request.Status.Should().Be(StreamingRequestStatus.Pending);
        request.CreatedAtUtc.Should().Be(createdAt);
        request.ExpiresAtUtc.Should().BeNull();
    }
    
    
    [Fact]
    public void Approve_WhenPending_ShouldSetApprovedStatusAndCalculatedExpiry()
    {
        // Arrange
        var request = new StreamingAccessRequest(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        var approvedAt = DateTime.UtcNow;
        var duration = TimeSpan.FromHours(2);

        // Act
        request.Approve(approvedAt, duration);

        // Assert
        request.Status.Should().Be(StreamingRequestStatus.Approved);
        request.ReviewedAtUtc.Should().Be(approvedAt);
        request.ExpiresAtUtc.Should().Be(approvedAt.Add(duration));
    }
    
    [Fact]
    public void Reject_WhenPending_ShouldSetRejectedStatusAndReason()
    {
        // Arrange
        var request = new StreamingAccessRequest(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        var reviewedAt = DateTime.UtcNow;
        var reason = "Uncertified tournament format.";

        // Act
        request.Reject(reason, reviewedAt);

        // Assert
        request.Status.Should().Be(StreamingRequestStatus.Rejected);
        request.ReviewedAtUtc.Should().Be(reviewedAt);
        request.RejectionReason.Should().Be(reason);
        request.ExpiresAtUtc.Should().BeNull();
    }
}