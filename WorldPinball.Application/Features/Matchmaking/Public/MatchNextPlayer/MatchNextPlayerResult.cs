namespace WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer;


public abstract record MatchNextPlayerResult{
private MatchNextPlayerResult() {}

public sealed record Success(Guid PlayerId, int SlotNumber, bool MatchFound) : MatchNextPlayerResult;
public sealed record NoPlayerInQueue : MatchNextPlayerResult;
public sealed record SessionNotFound(Guid SessionId) : MatchNextPlayerResult;
public sealed record SessionFull(Guid SessionId) : MatchNextPlayerResult;
};