namespace WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;

public sealed record LeaveMatchmakingQueueCommand(Guid MachineId, Guid PlayerId);
