using WorldPinball.Application.Features.StreamingAccess.Admin.Commands;

namespace WorldPinball.Application.Features.StreamingAccess.Admin;

public interface IApproveStreamingAccessHandler
{
    Task<ApproveStreamingAccessResult> HandleAsync(ApproveStreamingAccessCommand command, CancellationToken token = default);
}