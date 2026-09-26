namespace WorldPinball.Application.Features.GameSessions.Public.LaunchSession;

public abstract record LaunchSessionResult
{
    private LaunchSessionResult() { }

    public sealed record Success(Guid SessionId) : LaunchSessionResult;
    public sealed record SessionNotFound(Guid SessionId) : LaunchSessionResult;
    public sealed record CannotStartSession(string Message) : LaunchSessionResult;
}
