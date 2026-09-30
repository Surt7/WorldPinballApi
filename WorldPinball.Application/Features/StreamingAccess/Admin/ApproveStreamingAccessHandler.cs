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
    public async Task<ApproveStreamingAccessResult> HandleAsync(ApproveStreamingAccessCommand command, CancellationToken token = default)
    {
        var request = await requestRepository.GetByIdAsync(command.RequestId, token);
        if (request is null)
        {
            logger.LogWarning("Streaming access request {RequestId} not found.", command.RequestId);
            return new ApproveStreamingAccessResult.RequestNotFound(command.RequestId);
        }

        try
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            request.Approve(nowUtc, command.Duration);
            await requestRepository.UpdateAsync(request, token);

            return new ApproveStreamingAccessResult.Success(request.RequestId, request.ExpiresAtUtc!.Value);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            logger.LogWarning(ex, "Cannot approve streaming request {RequestId}: {Message}", command.RequestId, ex.Message);
            return new ApproveStreamingAccessResult.CannotApprove(ex.Message);
        }
    }
}