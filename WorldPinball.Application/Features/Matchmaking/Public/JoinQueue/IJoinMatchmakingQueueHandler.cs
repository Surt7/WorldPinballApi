using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;

namespace WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;

public interface IJoinMatchmakingQueueHandler
{
    Task<JoinMatchmakingQueueResult> HandleAsync(
        JoinMatchmakingQueueCommand command, 
        CancellationToken ct = default);
}