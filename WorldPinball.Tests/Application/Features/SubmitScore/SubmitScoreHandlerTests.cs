using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.SubmitScore;

public class SubmitScoreHandlerTests
{
    
    [Fact]
    public async Task HandleAsync_WhenSessionDoesNotExist_ShouldReturnSessionNotFound()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);
        
        var command = new SubmitScoreCommand(Guid.NewGuid(), Guid.NewGuid(), 50000);
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
                );
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionNotFound>();
    }

    [Fact]
    public async Task HandleAsync_WhenSessionIsEnded_ShouldReturnResultSessionAlreadyEnded()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        

       var timeProvider = TimeProvider.System;
       var now = timeProvider.GetUtcNow();
       var start = now.AddMinutes(-10);
       var end = now.AddMinutes(50);
       var sessionId = Guid.NewGuid();
       var machineId = Guid.NewGuid();
       var deviceId = Guid.NewGuid();
       var modeId = Guid.NewGuid();
       string clientVersion = "1.0.0";
       string nonce = "nonce";
       var gameSession = new GameSession(
           machineId,
           deviceId,
           modeId,
           clientVersion,
           nonce,
           now.UtcDateTime
       );
       gameSession.Start(start.UtcDateTime);
       gameSession.End(end.UtcDateTime);
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();
        var command = new SubmitScoreCommand(Guid.NewGuid(), deviceId, 50000);
        
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
            );
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionAlreadyEnded>();
    }

    [Fact]
    public async Task HandleAsync_DeviceDoesntMatch_ReturnResultDeviceMisMatch()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();

        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2; // not the capacity, it's the attributed place
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.Start(start.UtcDateTime);
        gameSession.AddPlayer(slotNumber,userId,"AAA");
        
        gameSession.End(end.UtcDateTime);
      
          writeRepository
                    .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                    .Returns(gameSession);

        
        var command = new SubmitScoreCommand(Guid.NewGuid(), Guid.NewGuid(), 50000,slotNumber);
        
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
            );
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
 
        result.Should().BeOfType<SubmitScoreResult.DeviceMisMatch>();
    }
    
    
    [Fact]
    public async Task HandleAsync_WheSlotIsNullOrEmpty_ShouldReturnResultDeviceMisMatch()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();

        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.Start(start.UtcDateTime);
        gameSession.End(end.UtcDateTime);
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);
        var command = new SubmitScoreCommand(Guid.NewGuid(), deviceId, 50000);
        
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
            );
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        result.Should().BeOfType<SubmitScoreResult.SessionAlreadyEnded>();
    }
    
    
     [Fact]
    public async Task HandleAsync_DeviceDoesntMatch_ReturnResultSlotNotFound()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();

        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2; // not the capacity, it's the attributed place
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.Start(start.UtcDateTime);
        gameSession.AddPlayer(slotNumber,userId,"AAA");
        
      
      
          writeRepository
                    .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                    .Returns(gameSession);

       
        var command = new SubmitScoreCommand(Guid.NewGuid(), deviceId, 50000);
       
        
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
            );
     
        var result = await sut.HandleAsync(command, CancellationToken.None);

        result.Should().BeOfType<SubmitScoreResult.SlotNotFound>();
    }
    
    
    [Fact]
    public async Task RequestIsValid_ShouldReturnResultSuccess()
    {
        var logger = new FakeLogger<SubmitScoreHandler>();
        var readRepository = Substitute.For<IGameSessionReadRepository>();
        var writeRepository = Substitute.For<IGameSessionWriteRepository>();
        var realtimeLeaderboardService = Substitute.For<IRealtimeLeaderboardService>();

        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();
        var start = now.AddMinutes(-10);
        var end = now.AddMinutes(50);
        var sessionId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        int slotNumber = 2; // not the capacity, it's the attributed place
        string clientVersion = "1.0.0";
        string nonce = "nonce";
        var gameSession = new GameSession(
            machineId,
            deviceId,
            modeId,
            clientVersion,
            nonce,
            now.UtcDateTime
        );
        gameSession.Start(start.UtcDateTime);
        gameSession.AddPlayer(slotNumber,userId,"AAA");
        
      
       
        writeRepository
            .GetSessionWithMachineAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gameSession);

  
        
        var command = new SubmitScoreCommand(gameSession.SessionId, deviceId, 50000, slotNumber);
        
        var sut = 
            new SubmitScoreHandler(readRepository,
                writeRepository,
                realtimeLeaderboardService,
                logger,
                TimeProvider.System
            );
        
        var result = await sut.HandleAsync(command, CancellationToken.None);
        
        await realtimeLeaderboardService.Received(1).AddScoreAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            50000,
            Arg.Any<CancellationToken>()
        );
        
        
        gameSession.State.Should().Be(GameSessionState.Ended);
        result.Should().BeOfType<SubmitScoreResult.Success>();
    }
    
    
    
}

