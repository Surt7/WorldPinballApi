using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Dtos;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class CreateGameSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var deviceId = Guid.NewGuid();
        var modeCode = "CLASSIC";
        var clientVersion = "1.0.0";
        var machineId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var logger = NullLogger<CreateGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        
        var deviceContext = new CreateGameSessionDto(
            DeviceId: deviceId,
            Status: DeviceStatus.Active,
            ActiveMachineId: machineId
        );
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(deviceContext);

        var gameMode = new GameMode(
            code: modeCode,
            name: "Classic Mode",
            isActive: true,
            modeId: modeId
        );
        repoGameSessionRead.GetActiveModeByCodeAsync(modeCode, Arg.Any<CancellationToken>())
            .Returns(gameMode);
        
        var command = new CreateGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: modeCode,
            ClientVersion: clientVersion
        );
        var sut = new CreateGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        result.Should().BeOfType<CreateGameSessionResult.Success>();
        var successResult = result as CreateGameSessionResult.Success;
        successResult.Should().NotBeNull();
        successResult!.Nonce.Should().NotBeNullOrWhiteSpace();
        successResult.SessionId.Should().NotBeEmpty();

        await repoGameSessionWrite.Received(1)
            .SaveSessionAsync(Arg.Is<GameSession>(s => 
                s.DeviceId == deviceId && 
                s.MachineId == machineId && 
                s.ModeId == modeId), 
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeviceNotFound_ShouldReturnDeviceNotFoundResult()
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var deviceId = Guid.NewGuid();
        var logger = NullLogger<CreateGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns((CreateGameSessionDto?)null);
        
        var command = new CreateGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: "CLASSIC",
            ClientVersion: "1.0.0"
        );
        var sut = new CreateGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        result.Should().BeOfType<CreateGameSessionResult.DeviceNotFound>();
        var notFoundResult = result as CreateGameSessionResult.DeviceNotFound;
        notFoundResult!.DeviceId.Should().Be(deviceId);
    }

    [Theory]
    [InlineData(DeviceStatus.Pending)]
    [InlineData(DeviceStatus.Retired)]
    [InlineData(DeviceStatus.Revoked)]
    public async Task HandleAsync_WhenDeviceNotActive_ShouldReturnDeviceNotActiveResult(DeviceStatus status)
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var deviceId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var logger = NullLogger<CreateGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        
        var deviceContext = new CreateGameSessionDto(
            DeviceId: deviceId,
            Status: status,
            ActiveMachineId: machineId
        );
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(deviceContext);
        
        var command = new CreateGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: "CLASSIC",
            ClientVersion: "1.0.0"
        );
        var sut = new CreateGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        result.Should().BeOfType<CreateGameSessionResult.DeviceNotActive>();
        var notActiveResult = result as CreateGameSessionResult.DeviceNotActive;
        notActiveResult!.DeviceId.Should().Be(deviceId);
    }

    [Fact]
    public async Task HandleAsync_WhenMachineNotAttached_ShouldReturnMachineNotAttachedResult()
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var deviceId = Guid.NewGuid();
        var logger = NullLogger<CreateGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        
        var deviceContext = new CreateGameSessionDto(
            DeviceId: deviceId,
            Status: DeviceStatus.Active,
            ActiveMachineId: null
        );
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(deviceContext);
        
        var command = new CreateGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: "CLASSIC",
            ClientVersion: "1.0.0"
        );
        var sut = new CreateGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        result.Should().BeOfType<CreateGameSessionResult.MachineNotAttached>();
        var notAttachedResult = result as CreateGameSessionResult.MachineNotAttached;
        notAttachedResult!.DeviceId.Should().Be(deviceId);
    }

    [Fact]
    public async Task HandleAsync_WhenModeNotFound_ShouldReturnModeNotFoundResult()
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var deviceId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var modeCode = "UNKNOWN_MODE";
        var logger = NullLogger<CreateGameSessionHandler>.Instance;
        var repoGameSessionWrite = Substitute.For<IGameSessionWriteRepository>();
        var repoGameSessionRead = Substitute.For<IGameSessionReadRepository>();
        
        var deviceContext = new CreateGameSessionDto(
            DeviceId: deviceId,
            Status: DeviceStatus.Active,
            ActiveMachineId: machineId
        );
        repoGameSessionRead.GetDeviceContextAsync(deviceId, Arg.Any<CancellationToken>())
            .Returns(deviceContext);

        repoGameSessionRead.GetActiveModeByCodeAsync(modeCode, Arg.Any<CancellationToken>())
            .Returns((GameMode?)null);
        
        var command = new CreateGameSessionCommand(
            DeviceId: deviceId,
            ModeCode: modeCode,
            ClientVersion: "1.0.0"
        );
        var sut = new CreateGameSessionHandler(
            repoGameSessionRead,
            repoGameSessionWrite,
            logger,
            timeProvider
        );

        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        result.Should().BeOfType<CreateGameSessionResult.ModeNotFound>();
        var modeNotFoundResult = result as CreateGameSessionResult.ModeNotFound;
        modeNotFoundResult!.ModeCode.Should().Be(modeCode);
    }
}
