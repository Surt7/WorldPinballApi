using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;

namespace WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;

public interface ILeaveMatchmakingQueueHandler
{
    Task<LeaveMatchmakingQueueResult> HandleAsync(
        LeaveMatchmakingQueueCommand command,
        CancellationToken ct = default);
}
