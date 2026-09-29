namespace WorldPinball.Application.Features.GameSessions.Public.AbortSession;

public abstract record AbortSessionResult
{
    private AbortSessionResult() { }
    public sealed record Success(Guid SessionId) : AbortSessionResult;
    public sealed record SessionNotFound(Guid SessionId) : AbortSessionResult;
    public sealed record CannotAbortSession(string Reason) : AbortSessionResult;
}