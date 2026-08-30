using WorldPinball.Application.Features.GameModes.Public.Dtos;

namespace WorldPinball.Application.Abstractions.Repositories
{
    public interface IGameModeRepository
    {
        Task<IReadOnlyList<GameModePublicDto>> GetGameModeAsync(CancellationToken cancellationToken = default);
    }
}
