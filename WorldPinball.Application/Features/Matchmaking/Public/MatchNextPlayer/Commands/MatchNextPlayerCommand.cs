namespace WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer.Commands;

public sealed record MatchNextPlayerCommand(
    Guid MachineId,
    Guid SessionId
    );