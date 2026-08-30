using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Features.GameModes.Public.Dtos;
using WorldPinball.Application.Abstractions.Repositories;

namespace WorldPinball.Infrastructure.Persistence.Repositories
{

    public sealed class GameModeRepository : IGameModeRepository
    {
        private readonly AppDbContext _db;

        public GameModeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<GameModePublicDto>> GetGameModeAsync(CancellationToken cancellationToken = default)
        {

            return await _db.GameModes
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new GameModePublicDto(
                    x.ModeId,
                    x.Code,
                    x.Name,
                    x.Description
                ))
                .ToListAsync(cancellationToken);

        }
    }

}
