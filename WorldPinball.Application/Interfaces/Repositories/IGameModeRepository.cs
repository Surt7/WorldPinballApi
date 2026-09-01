using WorldPinball.Application.Features.GameModes.Public.Dtos;

namespace WorldPinball.Application.Interfaces.Repositories
{
    public interface IGameModeRepository
    {
        Task<IReadOnlyList<GameModePublicDto>> GetGameModeAsync(CancellationToken cancellationToken = default);
    }
}
