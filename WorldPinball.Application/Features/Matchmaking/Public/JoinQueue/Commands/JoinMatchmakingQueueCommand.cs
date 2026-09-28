namespace WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;

public sealed record JoinMatchmakingQueueCommand(Guid MachineId, Guid PlayerId);