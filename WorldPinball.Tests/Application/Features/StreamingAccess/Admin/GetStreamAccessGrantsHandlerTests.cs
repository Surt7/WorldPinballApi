using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants;
using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Queries;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.StreamingAccess.Admin;

public class GetStreamAccessGrantsHandlerTests
{
    private readonly IStreamAccessGrantRepository _repository = Substitute.For<IStreamAccessGrantRepository>();
    private readonly NullLogger<GetStreamAccessGrantsHandler> _logger = NullLogger<GetStreamAccessGrantsHandler>.Instance;
    private readonly GetStreamAccessGrantsHandler _sut;

    public GetStreamAccessGrantsHandlerTests()
    {
        _sut = new GetStreamAccessGrantsHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenGrantsExist_ShouldMapAndReturnSuccess()
    {
        // Arrange
        var grant1 = new StreamAccessGrant(
            PrincipalType.Organization,
            Guid.NewGuid(),
            " Stern Pro Circuit ",
            GrantOrigin.Whitelist,
            DateTime.UtcNow);

        var grant2 = new StreamAccessGrant(
            PrincipalType.User,
            Guid.NewGuid(),
            "StreamerPro",
            GrantOrigin.ByApproval,
            DateTime.UtcNow);

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([grant1, grant2]);

        // Act
        var result = await _sut.HandleAsync(new GetStreamAccessGrantsQuery());

        // Assert
        result.Should().BeOfType<GetStreamAccessGrantsResult.Success>();
        var success = (GetStreamAccessGrantsResult.Success)result;
        success.Grants.Should().HaveCount(2);

        var dto1 = success.Grants.First(g => g.GrantId == grant1.GrantId);
        dto1.DisplayName.Should().Be("Stern Pro Circuit");
        dto1.PrincipalType.Should().Be("Organization");
        dto1.Origin.Should().Be("Whitelist");
        dto1.Status.Should().Be("Active");
    }

    [Fact]
    public async Task HandleAsync_WhenNoGrantsExist_ShouldReturnEmptyList()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _sut.HandleAsync(new GetStreamAccessGrantsQuery());

        // Assert
        result.Should().BeOfType<GetStreamAccessGrantsResult.Success>()
            .Which.Grants.Should().BeEmpty();
    }
}
