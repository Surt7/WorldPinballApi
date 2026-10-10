using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.StreamingAccess.Admin.Commands;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.StreamingAccess.Admin;

public sealed class ApproveStreamingAccessHandler(
    IStreamingAccessRequestRepository requestRepository,
    TimeProvider timeProvider,
    ILogger<ApproveStreamingAccessHandler> logger
) : IApproveStreamingAccessHandler
{
    private static readonly EventId RequestApproved = new(3430, nameof(RequestApproved));
    private static readonly EventId RequestNotFound = new(3431, nameof(RequestNotFound));
    private static readonly EventId ApprovalFailed = new(3432, nameof(ApprovalFailed));

    public async Task<ApproveStreamingAccessResult> HandleAsync(ApproveStreamingAccessCommand command, CancellationToken token = default)
    {
        var request = await requestRepository.GetByIdAsync(command.RequestId, token);
        if (request is null)
        {
            logger.LogWarning(RequestNotFound, "Streaming access request {RequestId} not found.", command.RequestId);
            return new ApproveStreamingAccessResult.RequestNotFound(command.RequestId);
        }

        try
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            request.Approve(nowUtc, command.Duration);
            await requestRepository.UpdateAsync(request, token);

            logger.LogInformation(RequestApproved, "Streaming access request {RequestId} approved until {ExpiresAtUtc}.",
                request.RequestId, request.ExpiresAtUtc);

            return new ApproveStreamingAccessResult.Success(request.RequestId, request.ExpiresAtUtc!.Value);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            logger.LogWarning(ApprovalFailed, ex, "Cannot approve streaming request {RequestId}: {Message}", command.RequestId, ex.Message);
            return new ApproveStreamingAccessResult.CannotApprove(ex.Message);
        }
    }
}