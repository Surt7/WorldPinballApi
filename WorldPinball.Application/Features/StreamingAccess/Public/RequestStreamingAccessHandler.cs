using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.StreamingAccess.Public.Commands;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.StreamingAccess.Public;

public sealed class RequestStreamingAccessHandler(
    IGameSessionWriteRepository sessionRepository,
    IStreamingAccessRequestRepository requestRepository,
    TimeProvider timeProvider,
    ILogger<RequestStreamingAccessHandler> logger
) : IRequestStreamingAccessHandler
{
    private static readonly EventId AccessRequested = new(3440, nameof(AccessRequested));
    private static readonly EventId SessionNotFound = new(3441, nameof(SessionNotFound));
    private static readonly EventId RequestAlreadyPending = new(3442, nameof(RequestAlreadyPending));

    public async Task<RequestStreamingAccessResult> HandleAsync(RequestStreamingAccessCommand command, CancellationToken token = default)
    {
        var session = await sessionRepository.GetSessionWithMachineAsync(command.SessionId, token);
        if (session is null)
        {
            logger.LogWarning(SessionNotFound, "Session {SessionId} not found for streaming request.", command.SessionId);
            return new RequestStreamingAccessResult.SessionNotFound(command.SessionId);
        }

        var existingRequest = await requestRepository.
            GetLatestRequestAsync(command.SessionId, token);
        
        if (existingRequest?.Status is Domain.Enums.StreamingRequestStatus.Pending)
        {
            logger.LogWarning(RequestAlreadyPending, "Streaming access request for session {SessionId} is already pending (RequestId: {RequestId}).",
                command.SessionId, existingRequest.RequestId);
            return new RequestStreamingAccessResult.RequestAlreadyPending(
                existingRequest.RequestId);
        }
        
        // Create new pending access request and persist
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var request = new Domain.Entities.StreamingAccessRequest(
            command.SessionId, command.RequestedByUserId, nowUtc);
        
        await requestRepository.AddAsync(request, token);

        logger.LogInformation(AccessRequested, "Created streaming access request {RequestId} for session {SessionId} by user {UserId}.",
            request.RequestId, command.SessionId, command.RequestedByUserId);
        
        return new RequestStreamingAccessResult.Success(request.RequestId, command.SessionId);
    }
}