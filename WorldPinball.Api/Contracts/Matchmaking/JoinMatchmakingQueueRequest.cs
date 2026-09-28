namespace WorldPinball.Api.Contracts.Matchmaking;

public sealed record JoinMatchmakingQueueRequest(Guid MachineId, Guid PlayerId);
