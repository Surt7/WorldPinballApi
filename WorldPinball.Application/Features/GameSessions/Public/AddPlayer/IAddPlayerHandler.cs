using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;


namespace WorldPinball.Application.Features.GameSessions.Public.AddPlayer;


public interface IAddPlayerHandler
{
    Task<AddPlayerResult> HandleAsync(
        AddPlayerCommand command, 
        CancellationToken cancellationToken = default
    );
}