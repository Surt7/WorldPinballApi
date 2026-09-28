using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Controllers.Public;
using WorldPinball.Application.Features.GameModes.Public.Dtos;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes.Queries;

namespace WorldPinball.Tests.Controller;

public class GameModesControllerTests
{
    private readonly IGetAllGameModesHandler _handler = Substitute.For<IGetAllGameModesHandler>();
    private readonly GameModesController _sut;

    public GameModesControllerTests()
    {
        _sut = new GameModesController(_handler);
    }

    [Fact]
    public async Task GetAll_WhenGameModesExist_ShouldReturnOk()
    {
        // Arrange
        var gameModes = new List<GameModePublicDto>
        {
            new(Guid.NewGuid(), "CLASSIC", "Classic", "Standard gameplay")
        };
        _handler.HandleAsync(Arg.Any<GetAllGameModesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetAllGameModesResult.Success(gameModes.AsReadOnly()));

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_WhenNoGameModesExist_ShouldReturnNotFound()
    {
        // Arrange
        _handler.HandleAsync(Arg.Any<GetAllGameModesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetAllGameModesResult.NotFound());

        // Act
        var result = await _sut.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
