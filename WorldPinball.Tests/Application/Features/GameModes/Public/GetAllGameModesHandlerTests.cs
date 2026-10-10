using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameModes.Public.Dtos;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Tests.Application.Features.GameModes.Public;

public class GetAllGameModesHandlerTests
{
    private readonly IGameModeRepository _gameModeRepo = Substitute.For<IGameModeRepository>();
    private readonly NullLogger<GetAllGameModesHandler> _logger = NullLogger<GetAllGameModesHandler>.Instance;
    private readonly GetAllGameModesHandler _sut;

    public GetAllGameModesHandlerTests()
    {
        _sut = new GetAllGameModesHandler(_gameModeRepo, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenGameModesExist_ShouldReturnSuccess()
    {
        // Arrange
        var gameModes = new List<GameModePublicDto>
        {
            new(Guid.NewGuid(), "CLASSIC", "Classic", "Standard gameplay"),
            new(Guid.NewGuid(), "TIMED", "Timed", "Beat the clock")
        };
        _gameModeRepo.GetGameModeAsync(Arg.Any<CancellationToken>())
            .Returns(gameModes.AsReadOnly());

        // Act
        var result = await _sut.HandleAsync(new GetAllGameModesQuery());

        // Assert
        result.Should().BeOfType<GetAllGameModesResult.Success>();
        var success = (GetAllGameModesResult.Success)result;
        success.GameModes.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoGameModesExist_ShouldReturnNotFound()
    {
        // Arrange
        _gameModeRepo.GetGameModeAsync(Arg.Any<CancellationToken>())
            .Returns(new List<GameModePublicDto>().AsReadOnly());

        // Act
        var result = await _sut.HandleAsync(new GetAllGameModesQuery());

        // Assert
        result.Should().BeOfType<GetAllGameModesResult.NotFound>();
    }
}
