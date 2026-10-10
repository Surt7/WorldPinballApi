using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant;
using WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.StreamingAccess.Admin;

public class RevokeStreamAccessGrantHandlerTests
{
    private readonly IStreamAccessGrantRepository _repository = Substitute.For<IStreamAccessGrantRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<RevokeStreamAccessGrantHandler> _logger = NullLogger<RevokeStreamAccessGrantHandler>.Instance;
    private readonly RevokeStreamAccessGrantHandler _sut;

    public RevokeStreamAccessGrantHandlerTests()
    {
        _sut = new RevokeStreamAccessGrantHandler(_repository, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldRevokeAndPersist()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "StreamerName",
            GrantOrigin.ByApproval,
            _timeProvider.GetUtcNow().UtcDateTime.AddDays(-5));

        _repository.GetByIdAsync(grant.GrantId, Arg.Any<CancellationToken>())
            .Returns(grant);

        var reason = "Repeated copyright infringement.";
        var command = new RevokeStreamAccessGrantCommand(grant.GrantId, reason);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RevokeStreamAccessGrantResult.Success>()
            .Which.GrantId.Should().Be(grant.GrantId);

        grant.Status.Should().Be(GrantStatus.Revoked);
        grant.RevocationReason.Should().Be(reason);
        await _repository.Received(1).UpdateAsync(grant, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenReasonIsNullOrWhitespace_ShouldReturnInvalidReason(string? invalidReason)
    {
        // Arrange
        var command = new RevokeStreamAccessGrantCommand(Guid.NewGuid(), invalidReason!);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RevokeStreamAccessGrantResult.InvalidReason>();
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGrantNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var grantId = Guid.NewGuid();
        _repository.GetByIdAsync(grantId, Arg.Any<CancellationToken>())
            .Returns((StreamAccessGrant?)null);

        var command = new RevokeStreamAccessGrantCommand(grantId, "Valid reason");

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RevokeStreamAccessGrantResult.NotFound>()
            .Which.GrantId.Should().Be(grantId);
    }

    [Fact]
    public async Task HandleAsync_WhenGrantAlreadyRevoked_ShouldReturnAlreadyRevoked()
    {
        // Arrange
        var grant = new StreamAccessGrant(
            PrincipalType.Organization,
            Guid.NewGuid(),
            "Old Partner",
            GrantOrigin.Whitelist,
            _timeProvider.GetUtcNow().UtcDateTime.AddDays(-10));

        grant.Revoke("Previous reason", _timeProvider.GetUtcNow().UtcDateTime.AddDays(-2));

        _repository.GetByIdAsync(grant.GrantId, Arg.Any<CancellationToken>())
            .Returns(grant);

        var command = new RevokeStreamAccessGrantCommand(grant.GrantId, "New reason");

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RevokeStreamAccessGrantResult.AlreadyRevoked>()
            .Which.GrantId.Should().Be(grant.GrantId);

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<StreamAccessGrant>(), Arg.Any<CancellationToken>());
    }
}
