
using WorldPinball.Api.Protos.V1.GameSessions;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;
using Grpc.Core;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer;

namespace WorldPinball.Api.Grpc;

public sealed class GameSessionGrpcService(
    IAddPlayerHandler addPlayerHandler,
    ILogger<GameSessionGrpcService> logger
) : GameSessionService.GameSessionServiceBase
{
    public override async Task<AddPlayerResponse> AddPlayer(AddPlayerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SessionId, out var sessionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session ID format"));

        Guid? userId = request.HasUserId && Guid.TryParse(request.UserId, out var parsed) ? parsed : null;
        string? initials = request.HasGuestInitials ? request.GuestInitials : null;

        var command = new AddPlayerCommand(sessionId, request.SlotNumber, userId, initials);
        var result = await addPlayerHandler.HandleAsync(command, context.CancellationToken);

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
}