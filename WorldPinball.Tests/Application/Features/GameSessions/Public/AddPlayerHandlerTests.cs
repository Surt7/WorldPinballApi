using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;



namespace WorldPinball.Tests.Application.Features.GameSessions.Public;

public class AddPlayerHandlerTests
{
    private readonly IGameSessionWriteRepository _writeRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly NullLogger<AddPlayerHandler> _logger = NullLogger<AddPlayerHandler>.Instance;

    [Fact]
    public async Task HandleAsync_WhenSessionExistsAndSlotAvailable_ShouldAddPlayerAndReturnSuccess()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
            );
        
        var userId = Guid.NewGuid();
   
        _writeRepo.GetSessionWithMachineAsync(
            sessionId,
            Arg.Any<CancellationToken>())
            .Returns(session);
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(sessionId, SlotNumber: 1,userId,GuestInitials: "AAA",RequiresRegisteredUser:false);
        
        
        // Act
        var result = await sut.HandleAsync(command);
        
        
        // Assert
        result.Should().BeOfType<AddPlayerResult.Success>().Which.SlotNumber.Should().Be(1);
        await _writeRepo.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenSessionDoesntExist_ShouldReturnResultSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
        );
        
        var userId = Guid.NewGuid();
        
      
        _writeRepo.GetSessionWithMachineAsync(
                sessionId,
                Arg.Any<CancellationToken>())
            .Returns((GameSession?)null);
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(sessionId, SlotNumber: 1,userId , GuestInitials: "AAA" , RequiresRegisteredUser: false);
        
        
        // Act
        var result = await sut.HandleAsync(command);
        
        
        // Assert
        result.Should().BeOfType<AddPlayerResult.SessionNotFound>();
   
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenSessionIsFullOrGameStateAreNotCreatedOrPending_ShouldReturnResultSessionNotJoinable()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
        );
        
        session.Launch(DateTime.UtcNow);
        session.End(DateTime.UtcNow); // GameSessionState.Ended
        
        _writeRepo.GetSessionWithMachineAsync(
                sessionId,
                Arg.Any<CancellationToken>())
            .Returns(session);
        
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(sessionId, SlotNumber: 1, GuestInitials: "AAA", RequiresRegisteredUser: false);
        
        
        // Act
        var result = await sut.HandleAsync(command);
        
        
        // Assert
     
        result.Should().BeOfType<AddPlayerResult.SessionNotJoinable>();

    }
    
    
    [Fact]
    public async Task HandleAsync_WhenTournamentRequiresRegisteredUserAndUserIdIsNull_ShouldReturnRegisteredUserRequired()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
        );
        
        
        _writeRepo.GetSessionWithMachineAsync(
                sessionId,
                Arg.Any<CancellationToken>())
            .Returns(session);
        
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(
            SessionId :sessionId,
            SlotNumber: 1,
            UserId: null, // not registred user
            GuestInitials: "AAA",
            RequiresRegisteredUser: true
            );
        
        
        // Act
        var result = await sut.HandleAsync(command);
        
        
        // Assert
     
        result.Should().BeOfType<AddPlayerResult.RegisteredUserRequired>();

    }
    
    
    [Fact]
    public async Task HandleAsync_WhenSlotIsInvalid_ShouldReturnInvalidSlot()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
        );
        
        
        _writeRepo.GetSessionWithMachineAsync(
                sessionId,
                Arg.Any<CancellationToken>())
            .Returns(session);
        
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(
            SessionId :sessionId,
            SlotNumber: 8,
            GuestInitials: "AAA",
            RequiresRegisteredUser: false
        );
        
        
        // Act
        var result = await sut.HandleAsync(command);
        
        
        // Assert
     
        result.Should().BeOfType<AddPlayerResult.InvalidSlot>();

    }
    
    
    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyRegistred_ShouldReturnUserAlreadyRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GameSession(Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            "1.0.0",
            "nonce",
            DateTime.UtcNow
        );
         
        var userId = Guid.NewGuid();
        
   
    
        
       var gameSession = _writeRepo.GetSessionWithMachineAsync(
                sessionId,
                Arg.Any<CancellationToken>())
            .Returns(session);
        
        
        var sut = new AddPlayerHandler(_writeRepo, _logger);
        var command = new AddPlayerCommand(
            SessionId :sessionId,
            UserId: userId,
            SlotNumber: 1,
            GuestInitials: "AAA",
            RequiresRegisteredUser: true
        );
        
        // We add player before in slot 1 to simulate the fact he is already registered in the current session
        session.AddPlayer(2,command.UserId,command.GuestInitials,true);
        // Act
        
        var result = await sut.HandleAsync(command);
        
       
        // Assert
     
        result.Should().BeOfType<AddPlayerResult.UserAlreadyRegistered>();

    }
    
}