using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant;
using WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.StreamingAccess.Admin;

public class CreateStreamAccessGrantHandlerTests
{
    private readonly IStreamAccessGrantRepository _repository = Substitute.For<IStreamAccessGrantRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<CreateStreamAccessGrantHandler> _logger = NullLogger<CreateStreamAccessGrantHandler>.Instance;
    private readonly CreateStreamAccessGrantHandler _sut;

    public CreateStreamAccessGrantHandlerTests()
    {
        _sut = new CreateStreamAccessGrantHandler(_repository, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldCreateAndPersistGrant()
    {
        // Arrange
        var command = new CreateStreamAccessGrantCommand(
            PrincipalType.Organization,
            Guid.NewGuid(),
            " Stern Pinball ",
            GrantOrigin.Whitelist,
            _timeProvider.GetUtcNow().UtcDateTime.AddYears(1),
            Guid.NewGuid());

        _repository.FindActiveGrantAsync(command.PrincipalType, command.PrincipalId, Arg.Any<CancellationToken>())
            .Returns((StreamAccessGrant?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateStreamAccessGrantResult.Success>();
        await _repository.Received(1).AddAsync(
            Arg.Is<StreamAccessGrant>(g =>
                g.PrincipalId == command.PrincipalId &&
                g.DisplayName == "Stern Pinball" &&
                g.Status == GrantStatus.Active),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPrincipalIdEmpty_ShouldReturnInvalidData()
    {
        // Arrange
        var command = new CreateStreamAccessGrantCommand(
            PrincipalType.User,
            Guid.Empty,
            "Valid Name",
            GrantOrigin.Whitelist);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateStreamAccessGrantResult.InvalidData>()
            .Which.Error.Should().Contain("PrincipalId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenDisplayNameIsNullOrWhitespace_ShouldReturnInvalidData(string? name)
    {
        // Arrange
        var command = new CreateStreamAccessGrantCommand(
            PrincipalType.User,
            Guid.NewGuid(),
            name!,
            GrantOrigin.Whitelist);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateStreamAccessGrantResult.InvalidData>()
            .Which.Error.Should().Contain("DisplayName");
    }

    [Fact]
    public async Task HandleAsync_WhenActiveGrantAlreadyExists_ShouldReturnAlreadyExists()
    {
        // Arrange
        var principalId = Guid.NewGuid();
        var existingGrant = new StreamAccessGrant(
            PrincipalType.Organization,
            principalId,
            "Existing Partner",
            GrantOrigin.Whitelist,
            _timeProvider.GetUtcNow().UtcDateTime);

        var command = new CreateStreamAccessGrantCommand(
            PrincipalType.Organization,
            principalId,
            "Duplicate Partner",
            GrantOrigin.Whitelist);

        _repository.FindActiveGrantAsync(command.PrincipalType, command.PrincipalId, Arg.Any<CancellationToken>())
            .Returns(existingGrant);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateStreamAccessGrantResult.AlreadyExists>()
            .Which.GrantId.Should().Be(existingGrant.GrantId);

        await _repository.DidNotReceive().AddAsync(Arg.Any<StreamAccessGrant>(), Arg.Any<CancellationToken>());
    }
}
