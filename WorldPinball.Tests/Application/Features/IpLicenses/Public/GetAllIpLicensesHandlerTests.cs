using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.IpLicenses.Public.Dtos;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Tests.Application.Features.IpLicenses.Public;

public class GetAllIpLicensesHandlerTests
{
    private readonly IIpLicenseReadRepository _ipLicenseRepo = Substitute.For<IIpLicenseReadRepository>();
    private readonly NullLogger<GetAllIpLicensesHandler> _logger = NullLogger<GetAllIpLicensesHandler>.Instance;
    private readonly GetAllIpLicensesHandler _sut;

    public GetAllIpLicensesHandlerTests()
    {
        _sut = new GetAllIpLicensesHandler(_ipLicenseRepo, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenIpLicensesExist_ShouldReturnSuccess()
    {
        // Arrange
        var ipLicenses = new List<IpLicenseDto>
        {
            new(Guid.NewGuid(), "Godzilla", "godzilla", new PublisherDto(Guid.NewGuid(), "Stern", "stern")),
            new(Guid.NewGuid(), "Iron Maiden", "iron-maiden", new PublisherDto(Guid.NewGuid(), "Stern", "stern"))
        };
        _ipLicenseRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ipLicenses.AsReadOnly());

        // Act
        var result = await _sut.HandleAsync(new GetAllIpLicensesQuery());

        // Assert
        result.Should().BeOfType<GetAllIpLicensesResult.Success>();
        var success = (GetAllIpLicensesResult.Success)result;
        success.IpLicenses.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoIpLicensesExist_ShouldReturnNotFound()
    {
        // Arrange
        _ipLicenseRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<IpLicenseDto>().AsReadOnly());

        // Act
        var result = await _sut.HandleAsync(new GetAllIpLicensesQuery());

        // Assert
        result.Should().BeOfType<GetAllIpLicensesResult.NotFound>();
    }
}
