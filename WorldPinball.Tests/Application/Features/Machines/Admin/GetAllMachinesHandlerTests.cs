using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin.GetAllMachines;
using WorldPinball.Application.Features.Machines.Admin.GetAllMachines.Queries;
using WorldPinball.Application.Features.Machines.Admin.Provision.Dtos;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.Machines.Admin;

public sealed class GetAllMachinesHandlerTests
{
    private readonly IMachineReadRepository _readRepository = Substitute.For<IMachineReadRepository>();
    private readonly ILogger<GetAllMachinesHandler> _logger = Substitute.For<ILogger<GetAllMachinesHandler>>();
    private readonly GetAllMachinesHandler _sut;

    public GetAllMachinesHandlerTests()
    {
        _sut = new GetAllMachinesHandler(_readRepository, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenMachinesExist_ShouldReturnSuccessWithListOfMachineRowDto()
    {
        // Arrange
        var machines = new List<MachineRowDto>
        {
            new(
                Guid.NewGuid(),
                "Medieval Madness",
                "MM-01",
                MachineStatus.Active,
                "Main arcade room",
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow,
                "WP-SN-001"),
            new(
                Guid.NewGuid(),
                "Attack from Mars",
                "AFM-01",
                MachineStatus.PendingEnrollment,
                null,
                DateTime.UtcNow.AddDays(-2),
                null,
                "WP-SN-002")
        };

        _readRepository.GetAllMachines(Arg.Any<CancellationToken>())
            .Returns(machines);

        // Act
        var result = await _sut.HandleAsync(new GetAllMachinesQuery(), CancellationToken.None);

        // Assert
        var success = result.Should().BeOfType<GetAllMachinesResult.Success>().Which;
        success.Machines.Should().HaveCount(2);
        success.Machines.Should().BeEquivalentTo(machines);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMachinesExist_ShouldReturnNotFound()
    {
        // Arrange
        _readRepository.GetAllMachines(Arg.Any<CancellationToken>())
            .Returns(new List<MachineRowDto>());

        // Act
        var result = await _sut.HandleAsync(new GetAllMachinesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetAllMachinesResult.NotFound>();
    }
}
