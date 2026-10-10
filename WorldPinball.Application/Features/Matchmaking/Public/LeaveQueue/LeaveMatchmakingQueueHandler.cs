using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;

public sealed class LeaveMatchmakingQueueHandler(
    IMatchmakingQueueService matchmakingQueueService,
    ILogger<LeaveMatchmakingQueueHandler> logger
) : ILeaveMatchmakingQueueHandler
{
    private static readonly EventId PlayerNotInQueueEvent = new(2701, nameof(PlayerNotInQueueEvent));

    public async Task<LeaveMatchmakingQueueResult> HandleAsync(
        LeaveMatchmakingQueueCommand cmd,
        CancellationToken ct = default)
    {
        var removed = await matchmakingQueueService.RemoveAsync(
            cmd.MachineId.ToString(),
            cmd.PlayerId,
            ct);

        if (!removed)
        {
            logger.LogWarning(
                PlayerNotInQueueEvent,
                "Matchmaking > Machine {MachineId}: Player {PlayerId} was not in queue.",
                cmd.MachineId,
                cmd.PlayerId);
            return new LeaveMatchmakingQueueResult.PlayerNotInQueue();
        }

        return new LeaveMatchmakingQueueResult.Success();
    }
}
