using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Public;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;

public sealed class JoinMatchmakingQueueHandler(
    IMachineReadRepository machineReadRepository,
    IMatchmakingQueueService matchmakingQueueService,
    ILogger<JoinMatchmakingQueueHandler> logger
)
    : IJoinMatchmakingQueueHandler
{
    private static readonly EventId MachineNotFound =
        new(2700, nameof(MachineNotFound));
    
    public async Task<JoinMatchmakingQueueResult> HandleAsync(JoinMatchmakingQueueCommand cmd, CancellationToken ct = default)
    {
        
        var machine = await machineReadRepository.GetMachinesWithId(cmd.MachineId, ct);
        
        if (machine is null)
        {
            logger.LogWarning(
                MachineNotFound,
                "Matchmaking > Machine {MachineId}: MachineId is empty.",
                cmd.MachineId);
            return new JoinMatchmakingQueueResult.MachineNotFound(cmd.MachineId);
        }
        
        await matchmakingQueueService.EnqueueAsync(
            cmd.MachineId.ToString(), 
            cmd.PlayerId, 
            ct);
        
        return new JoinMatchmakingQueueResult.Success();
    }
}