using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure.Services.Authorization;

namespace WorldPinball.Tests.Infrastructure.Services;

public class StreamingAuthorizationServiceTests
{
    private readonly IStreamingPartnerRepository _partnerRepo = Substitute.For<IStreamingPartnerRepository>();
    private readonly IStreamingAccessRequestRepository _requestRepo = Substitute.For<IStreamingAccessRequestRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 30, 16, 0, 0, TimeSpan.Zero));
    private readonly StreamingAuthorizationService _sut;

    public StreamingAuthorizationServiceTests()
    {
        _sut = new StreamingAuthorizationService(_partnerRepo, _requestRepo, _timeProvider);
    }

    [Fact]
    public async Task CanStreamAsync_WhenUserIsActivePartner_ShouldReturnTrueWithoutCheckingRequests()
    {
        // Arrange: Partner on whitelist
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _partnerRepo.IsActivePartnerAsync(userId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var canStream = await _sut.CanStreamAsync(userId, sessionId);

        // Assert
        canStream.Should().BeTrue();
        await _requestRepo.DidNotReceive().GetLatestRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task CanStreamAsync_WhenUserIsNotPartnerButHasValidApprovedRequest_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var request = new StreamingAccessRequest(sessionId, userId, _timeProvider.GetUtcNow().UtcDateTime);
        request.Approve(_timeProvider.GetUtcNow().UtcDateTime, TimeSpan.FromHours(2));

        _partnerRepo.IsActivePartnerAsync(userId, Arg.Any<CancellationToken>()).Returns(false);
        _requestRepo.GetLatestRequestAsync(sessionId, Arg.Any<CancellationToken>()).Returns(request);

        // Act
        var canStream = await _sut.CanStreamAsync(userId, sessionId);

        // Assert
        canStream.Should().BeTrue();
    }
    
    
    [Fact]
    public async Task CanStreamAsync_WhenUserIsNotPartnerAndRequestIsExpired_ShouldReturnFalse()
    {
        // Arrange: lease expired 10 minutes ago
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var createdAt = _timeProvider.GetUtcNow().UtcDateTime.AddHours(-3);
        var request = new StreamingAccessRequest(sessionId, userId, createdAt);
        request.Approve(createdAt, TimeSpan.FromHours(2)); // Expired at -1 hour

        _partnerRepo.IsActivePartnerAsync(userId, Arg.Any<CancellationToken>()).Returns(false);
        _requestRepo.GetLatestRequestAsync(sessionId, Arg.Any<CancellationToken>()).Returns(request);

        // Act
        var canStream = await _sut.CanStreamAsync(userId, sessionId);

        // Assert
        canStream.Should().BeFalse();
    }
}