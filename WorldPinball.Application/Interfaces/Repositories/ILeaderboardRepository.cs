using WorldPinball.Application.Features.Leaderboard.Public.Dtos;


namespace WorldPinball.Application.Interfaces.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<IReadOnlyList<LeaderboardRowDto>> GetLeaderboardAsync(
            string? ipSlug = null,
            string? modeCode = null,
            int limit = 50,
            CancellationToken cancellationToken = default);
    }
}
