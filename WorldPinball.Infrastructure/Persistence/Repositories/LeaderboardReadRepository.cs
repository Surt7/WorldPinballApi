using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class LeaderboardReadRepository : ILeaderboardReadRepository
{
    private readonly AppDbContext _db;

    public LeaderboardReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LeaderboardRowDto>> GetLeaderboardAsync(
        Guid? ipId = null,
        Guid? modeId = null,
        string? ipSlug = null,
        string? modeCode = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var q = _db.ScoreEntries
            .AsNoTracking()
            .Where(x => x.IsValidated);

        if (ipId.HasValue && ipId != Guid.Empty)
        {
            q = q.Where(x => x.IpId == ipId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(ipSlug))
        {
            q = q.Where(x => x.IpLicense.Slug == ipSlug);
        }

        if (modeId.HasValue && modeId != Guid.Empty)
        {
            q = q.Where(x => x.ModeId == modeId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(modeCode))
        {
            q = q.Where(x => x.Mode.Code == modeCode);
        }

        var entries = await q
            .OrderByDescending(x => x.Score)
            .ThenBy(at => at.AchievedAtUtc) // second orderding by ASC default
            .Take(limit)
            .Select(x => new
            {
                x.ScoreEntryId,
                x.Score,
                x.AchievedAtUtc,
                IpName = x.IpLicense.Name,
                ModeName = x.Mode.Name,
                Player = x.Session.Players
                    .FirstOrDefault(p => p.SlotNumber == x.SlotNumber)
            })
            .ToListAsync(cancellationToken);
        
        // I done loop to avoid double trigger ToList()
        // problem: we had to map rank with index (+ avoid tricky Object[index] indexer)

        var result = new List<LeaderboardRowDto>(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            var item = entries[i];
            result.Add(new LeaderboardRowDto(
                Rank: i + 1,
                ScoreEntryId: item.ScoreEntryId,
                PlayerInitials: item.Player?.GuestInitials ?? item.Player?.UserProfile?.Initials ?? "AAA",
                PlayerName: item.Player?.UserProfile?.DisplayName ?? "Guest",
                Score: item.Score,
                AchievedAtUtc: item.AchievedAtUtc,
                IpName: item.IpName,
                ModeName: item.ModeName
            ));
        }


        return result;
    }


}
