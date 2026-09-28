using WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer.Commands;

namespace WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer;

public interface IMatchNextPlayerHandler
{
    Task<MatchNextPlayerResult> HandleAsync(MatchNextPlayerCommand command, CancellationToken cancellationToken = default);
}