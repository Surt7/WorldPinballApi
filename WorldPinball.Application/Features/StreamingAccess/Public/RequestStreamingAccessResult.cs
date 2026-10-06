namespace WorldPinball.Application.Features.StreamingAccess.Public;

public abstract record RequestStreamingAccessResult
{
    private RequestStreamingAccessResult() { }
    
    public sealed record Success(Guid RequestId, Guid SessionId) : RequestStreamingAccessResult;

    public sealed record SessionNotFound(Guid SessionId) : RequestStreamingAccessResult;
    
    public sealed record RequestAlreadyPending(Guid RequestId) : RequestStreamingAccessResult;
}