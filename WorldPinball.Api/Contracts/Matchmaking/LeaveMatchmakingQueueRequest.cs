namespace WorldPinball.Api.Contracts.Matchmaking;

public sealed record LeaveMatchmakingQueueRequest(Guid MachineId, Guid PlayerId);
