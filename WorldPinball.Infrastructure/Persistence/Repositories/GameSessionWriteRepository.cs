using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;


namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class GameSessionWriteRepository(AppDbContext db) : IGameSessionWriteRepository
{
    // Save Session
    public async Task SaveSessionAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        await db.GameSessions.AddAsync(session, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    
    //Update Session
    public async Task UpdateSessionAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    // Get Session
    public async Task<GameSession?> GetSessionWithMachineAsync(Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.GameSessions
            .Where(gs => gs.SessionId == sessionId)
            .Include(gs => gs.Machine)
            .Include(gs => gs.Players)
            .FirstOrDefaultAsync(cancellationToken);
    }

    
    public async Task SaveScoreEntryAsync(GameSession session, ScoreEntry scoreEntry,
        CancellationToken cancellationToken = default)
    {
        await db.ScoreEntries.AddAsync(scoreEntry, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Submit all score
    public async Task SaveScoreEntriesAsync(GameSession session, IEnumerable<ScoreEntry> scoreEntries,
        CancellationToken cancellationToken = default)
    {
        await db.ScoreEntries.AddRangeAsync(scoreEntries, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}