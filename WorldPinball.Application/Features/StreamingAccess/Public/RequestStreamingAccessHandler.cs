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
    public async Task<RequestStreamingAccessResult> HandleAsync(RequestStreamingAccessCommand command, CancellationToken token = default)
    {
        var session = await sessionRepository.GetSessionWithMachineAsync(command.SessionId, token);
        if (session is null)
        {
            logger.LogWarning("Session {SessionId} not found for streaming request.", command.SessionId);
            return new RequestStreamingAccessResult.SessionNotFound(command.SessionId);
        }

        var existingRequest = await requestRepository.
            GetLatestRequestAsync(command.SessionId, token);
        
        if (existingRequest?.Status is Domain.Enums.StreamingRequestStatus.Pending)
        {
            return new RequestStreamingAccessResult.RequestAlreadyPending(
                existingRequest.RequestId);
        }
        
        // Create new pending access request and persist
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var request = new Domain.Entities.StreamingAccessRequest(
            command.SessionId, command.RequestedByUserId, nowUtc);
        
        await requestRepository.AddAsync(request, token);
        
        return new RequestStreamingAccessResult.Success(Guid.NewGuid(), command.SessionId);
    }
}