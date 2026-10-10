namespace WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;

public abstract record LeaveMatchmakingQueueResult
{
    private LeaveMatchmakingQueueResult() { }
    public sealed record Success : LeaveMatchmakingQueueResult;
    public sealed record PlayerNotInQueue : LeaveMatchmakingQueueResult;
}
