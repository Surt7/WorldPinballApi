using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Controllers.Public;
using WorldPinball.Application.Features.IpLicenses.Public.Dtos;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses.Queries;

namespace WorldPinball.Tests.Controller;

public class IpLicensesControllerTests
{
    private readonly IGetAllIpLicensesHandler _handler = Substitute.For<IGetAllIpLicensesHandler>();
    private readonly IpLicensesController _sut;

    public IpLicensesControllerTests()
    {
        _sut = new IpLicensesController(_handler);
    }

    [Fact]
    public async Task GetAll_WhenIpLicensesExist_ShouldReturnOk()
    {
        // Arrange
        var ipLicenses = new List<IpLicenseDto>
        {
            new(Guid.NewGuid(), "Godzilla", "godzilla", new PublisherDto(Guid.NewGuid(), "Stern", "stern"))
        };
        _handler.HandleAsync(Arg.Any<GetAllIpLicensesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetAllIpLicensesResult.Success(ipLicenses.AsReadOnly()));

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_WhenNoIpLicensesExist_ShouldReturnNotFound()
    {
        // Arrange
        _handler.HandleAsync(Arg.Any<GetAllIpLicensesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetAllIpLicensesResult.NotFound());

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
