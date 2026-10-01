namespace WorldPinball.Application.Features.StreamingAccess.Admin;

public abstract record ApproveStreamingAccessResult
{
    private ApproveStreamingAccessResult() { }
    
    public sealed record Success(Guid RequestId,
        DateTime ExpiresAtUtc) : ApproveStreamingAccessResult;
    
    public sealed record RequestNotFound(Guid RequestId) : ApproveStreamingAccessResult;

    public sealed record CannotApprove(string Reason) : ApproveStreamingAccessResult;
}