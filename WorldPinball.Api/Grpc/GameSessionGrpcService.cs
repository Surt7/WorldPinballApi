
using WorldPinball.Api.Protos.V1.GameSessions;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;
using Grpc.Core;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Dtos;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Queries;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Api.Grpc;

public sealed class GameSessionGrpcService(
    IAddPlayerHandler addPlayerHandler,
    IKickPlayerHandler kickPlayerHandler,
    IGetLobbyHandler getLobbyHandler,
    ILobbyNotificationService lobbyNotifier,
    ILogger<GameSessionGrpcService> logger
) : GameSessionService.GameSessionServiceBase
{
    public override async Task<AddPlayerResponse> AddPlayer(
        AddPlayerRequest request,
        ServerCallContext context
        )
    {
        
        if (!Guid.TryParse(request.SessionId, out var sessionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID format"));

        Guid? userId = request.HasUserId && Guid.TryParse(request.UserId, out var parsed) ? parsed : null;
        string? initials = request.HasGuestInitials ? request.GuestInitials : null;
        
        var command = new AddPlayerCommand(sessionId, request.SlotNumber, userId, initials);
        var result = await addPlayerHandler.HandleAsync(command, context.CancellationToken);

        if (result is AddPlayerResult.Success)
        {
            lobbyNotifier.NotifyLobbyUpdated(sessionId);
        }
        
        logger.LogInformation("Added player to slot {Slot} for session {SessionId}.", command.SlotNumber, command.SessionId);
        
        return result switch
        {
            AddPlayerResult.Success s =>
                new AddPlayerResponse { Success = true, SlotNumber = s.SlotNumber, Message = "Player added." },

            AddPlayerResult.SessionNotFound =>
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found.")),

            AddPlayerResult.SessionNotJoinable =>
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, "Session is not joinable.")),

            AddPlayerResult.SlotAlreadyOccupied =>
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Slot is already occupied.")),

            AddPlayerResult.InvalidSlot =>
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Slot number must be between 1 and 4.")),

            AddPlayerResult.RegisteredUserRequired =>
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Registered user required.")),

            AddPlayerResult.UserAlreadyRegistered =>
                throw new RpcException(new Status(StatusCode.AlreadyExists, "User already registered.")),

            _ => throw new RpcException(new Status(StatusCode.Internal,
                "Unexpected error."))
            
            
            
        };
    }


    public override async Task<KickPlayerResponse> KickPlayer(KickPlayerRequest request, ServerCallContext context)
    {
        
        if (!Guid.TryParse(request.SessionId, out var sessionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID format"));
        
        var command = new KickPlayerCommand(
            sessionId, 
            request.SlotNumber, 
            request.HasReason ? request.Reason : null);
        var result = await kickPlayerHandler.HandleAsync(command, context.CancellationToken);
  
        if (result is KickPlayerResult.Success)
        {
            lobbyNotifier.NotifyLobbyUpdated(sessionId);
        }
        
        return result switch
        {
            KickPlayerResult.Success s =>
                new KickPlayerResponse { Success = true, SlotNumber = s.SlotNumber, Message = "Player kicked." },
            
            KickPlayerResult.SessionNotFound =>
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found.")),
            
            KickPlayerResult.CannotRemovePlayerActiveSession =>
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Cannot remove player once session has started or ended.")),
            
            KickPlayerResult.PlayerNotFound =>
                throw new RpcException(new Status(StatusCode.NotFound, "Player not found in the specified slot.")),
            
            KickPlayerResult.InvalidSlot =>
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Slot number must be between 1 and 4.")),
            
            //Default
            _ => throw new RpcException(new Status(StatusCode.Internal, "Unexpected error."))
        };
    }
    
    
    
    public override async Task<SessionLobbyResponse> GetLobby(GetLobbyRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SessionId, out var sessionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID format"));

        var query = new GetLobbyQuery(sessionId);
        var result = await getLobbyHandler.HandleAsync(query, context.CancellationToken);

        return result switch
        {
            GetLobbyResult.Success s => MapToLobbyResponse(s.Lobby),

            GetLobbyResult.SessionNotFound =>
                throw new RpcException(new Status(StatusCode.NotFound, "Session not found.")),

            _ => throw new RpcException(new Status(StatusCode.Internal, "Unexpected error."))
        };
    }

    
    //Mapping
    private static SessionLobbyResponse MapToLobbyResponse(SessionLobbyDto lobby)
    {
        var response = new SessionLobbyResponse
        {
            SessionId = lobby.SessionId.ToString(),
            SessionState = lobby.SessionState
        };

        foreach (var slot in lobby.Slots)
        {
            var slotInfo = new PlayerSlotInfo
            {
                SlotNumber = slot.SlotNumber,
                IsOccupied = slot.IsOccupied
            };

            if (slot.UserId.HasValue)
                slotInfo.UserId = slot.UserId.Value.ToString();

            if (!string.IsNullOrWhiteSpace(slot.GuestInitials))
                slotInfo.GuestInitials = slot.GuestInitials;

            response.Slots.Add(slotInfo);
        }

        return response;
    }



    public override async Task WatchLobby(
        WatchLobbyRequest request,
        IServerStreamWriter<SessionLobbyResponse> responseStream,
        ServerCallContext context)
    {

        if (!Guid.TryParse(request.SessionId, out var sessionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID format"));
        
        // send initial snapshot
        var initialResult = await getLobbyHandler.HandleAsync(new GetLobbyQuery(sessionId), context.CancellationToken);
        if (initialResult is not GetLobbyResult.Success initialSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, "Session not found."));
        
        await responseStream.WriteAsync(MapToLobbyResponse(initialSuccess.Lobby));
     
        
        if (initialSuccess.Lobby.SessionState is "Started" or "Closed" or "Cancelled")
            return;
        
        await foreach (var _ in lobbyNotifier.SubscribeAsync(sessionId, context.CancellationToken))
        {
            var result = await getLobbyHandler.HandleAsync(new GetLobbyQuery(sessionId), context.CancellationToken);
            if (result is GetLobbyResult.Success success)
            {
                await responseStream.WriteAsync(MapToLobbyResponse(success.Lobby));
                if (success.Lobby.SessionState is "Started" or "Closed" or "Cancelled")
                    break;
            }
        }
    }




}
