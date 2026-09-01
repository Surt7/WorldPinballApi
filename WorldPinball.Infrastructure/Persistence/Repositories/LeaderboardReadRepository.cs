using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class LeaderboardRepository : ILeaderboardRepository
{
    private readonly AppDbContext _db;

    public LeaderboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LeaderboardRowDto>> GetLeaderboardAsync(
        string? ipSlug,
        string? modeCode,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var q = _db.ScoreEntries
            .AsNoTracking()
            .Where(x => x.IsValidated);

        if (!string.IsNullOrWhiteSpace(ipSlug))
        {
            q = q.Where(x => x.IpLicense.Slug == ipSlug);
        }

        if (!string.IsNullOrWhiteSpace(modeCode))
        {
            q = q.Where(x => x.Mode.Code == modeCode);
        }

        return await q
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.AchievedAtUtc)
            .ThenByDescending(x => x.ScoreEntryId)
            .Take(limit)
            .Select(x => new LeaderboardRowDto(
                x.ScoreEntryId,
                x.IpLicense.Slug,
                x.Mode.Code,
                x.Score,
                x.AchievedAtUtc,
                x.UserId ?? string.Empty,
                x.MachineId
            ))
            .ToListAsync(cancellationToken);
    }
}
