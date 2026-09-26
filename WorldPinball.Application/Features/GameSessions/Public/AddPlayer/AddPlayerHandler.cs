using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.AddPlayer;

public sealed class AddPlayerHandler(
    IGameSessionWriteRepository writeRepository,
    ILogger<AddPlayerHandler> logger
    ):IAddPlayerHandler
{
    private static readonly EventId SessionNotFound = new EventId(2500, nameof(SessionNotFound));
    private static readonly EventId SessionNotJoinable = new EventId(2501, nameof(SessionNotJoinable));
    private static readonly EventId SlotAlreadyOccupied = new EventId(2502, nameof(SlotAlreadyOccupied));    
    private static readonly EventId RegisteredUserRequired = new EventId(2503, nameof(RegisteredUserRequired));
    private static readonly EventId InvalidSlot = new EventId(2504, nameof(InvalidSlot));
    private static readonly EventId UserAlreadyRegistered = new EventId(2505, nameof(UserAlreadyRegistered));
    
    public async Task<AddPlayerResult> HandleAsync(
        AddPlayerCommand command,
        CancellationToken cancellationToken = default
        )
    {
        
        var session = await writeRepository
            .GetSessionWithMachineAsync(
                command.SessionId,
                cancellationToken
                );

        if (session is null)
        {
            
            logger.LogWarning(
                SessionNotFound,
                "Game session : {SessionId}  could not be found.",
                command.SessionId
                );
            
            return new AddPlayerResult.SessionNotFound(command.SessionId);
        }

        if (session.State is not
            (GameSessionState.Created 
            or GameSessionState.Pending
            or GameSessionState.SearchingPlayer
            or GameSessionState.Joined
            or GameSessionState.Started
            or GameSessionState.Resumed
            )
            )
        {
            logger.LogWarning(
                SessionNotJoinable,
                "Session {SessionId} is in non-joinable state {State}.",
                session.SessionId, session.State
                );
            return new AddPlayerResult.SessionNotJoinable(session.SessionId);
        }

        if (command is { SlotNumber: > 4 or < 1 })
        {
            logger.LogWarning(
                InvalidSlot,
                "Slot {Slot} in session {SessionId} is invalid.",
                command.SlotNumber, session.SessionId
            );
            return new AddPlayerResult.InvalidSlot(command.SlotNumber);
        }
        
        
        if (session.Players.Any(p => p.SlotNumber == command.SlotNumber))
        {
            logger.LogWarning(SlotAlreadyOccupied,
                "Slot {Slot} in session {SessionId} is already occupied.",
                command.SlotNumber, session.SessionId
                );
            return new AddPlayerResult.SlotAlreadyOccupied(command.SlotNumber);
        }

        if (command is { RequiresRegisteredUser: true, UserId: null })
        {
            logger.LogWarning(
                RegisteredUserRequired,
                "Registered User is required  in session {SessionId} ",
                 session.SessionId
                );
            return new AddPlayerResult.RegisteredUserRequired(command.SessionId);
        }

        if (command.UserId.HasValue && session.Players.Any(p => p.UserId == command.UserId.Value))
        {
            logger.LogWarning(
                UserAlreadyRegistered,
                "User {UserId} already registered in session {SessionId}.",
                command.UserId.Value, session.SessionId
            );
            return new AddPlayerResult.UserAlreadyRegistered(command.UserId.Value);
        }
        
        session.AddPlayer(
            command.SlotNumber,
            command.UserId,
            command.GuestInitials,
            command.RequiresRegisteredUser);
    

        await writeRepository.UpdateSessionAsync(session, cancellationToken);
        
        
        return new AddPlayerResult.Success(session.SessionId, command.SlotNumber);
        
       
       
    }
}