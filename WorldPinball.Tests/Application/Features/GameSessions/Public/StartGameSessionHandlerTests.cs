using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameSesions.Public.Start;
using WorldPinball.Application.Features.GameSesions.Public.Start.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class StartGameSessionHandlerTests
{


    [Fact(Skip = "WIP: En attente d'implémentation du StartGameSessionHandler")]
    public async Task HandleAsync_WhenDeviceIdIsNotNullOrWhitespace_ShouldReturnSuccessResult()
    {
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var deviceId = Guid.NewGuid();
        var modeCode = "CLASSIC";
        var userId = "player-42";
        var clientVersion = "1.0.0";
        var machineId = Guid.NewGuid();
        var logger = NullLogger<StartGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        
        var deviceContext = new DeviceSessionContext(
            DeviceId: deviceId,
            Status: DeviceStatus.Active,
            ActiveMachineId: machineId
        );
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(deviceContext);
        
        var command = new StartGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: modeCode,
            UserId: userId,
            ClientVersion: clientVersion
        );
        var sut = new StartGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        var result = await sut.HandleAsync(command);
        
        result.Should().BeOfType<StartGameSessionResult.Success>();


    }
    
    
}