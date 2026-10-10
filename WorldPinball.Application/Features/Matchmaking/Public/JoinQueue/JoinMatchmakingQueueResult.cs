namespace WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;

public abstract record JoinMatchmakingQueueResult
{
    private JoinMatchmakingQueueResult() { }
    public sealed record Success : JoinMatchmakingQueueResult;
    public sealed record MachineNotFound(Guid MachineId) : JoinMatchmakingQueueResult;
}