using WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.KickPlayer;

public interface IKickPlayerHandler
{
    Task<KickPlayerResult> HandleAsync(
        KickPlayerCommand command, 
        CancellationToken cancellationToken = default
    );
}