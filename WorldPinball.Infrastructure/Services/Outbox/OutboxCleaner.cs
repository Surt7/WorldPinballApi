using Microsoft.EntityFrameworkCore;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Services.Outbox;


public interface IOutboxCleaner
{
    Task<int> CleanupBatchAsync(TimeSpan retentionPeriod, int batchSize = 100, CancellationToken ct = default);
}

public class OutboxCleaner(
    AppDbContext db,
    TimeProvider timeProvider
    ):IOutboxCleaner
{
    public async Task<int> CleanupBatchAsync(TimeSpan retentionPeriod, int batchSize = 100, CancellationToken ct = default)
    {
        
        var cutoff = timeProvider.GetUtcNow().UtcDateTime - retentionPeriod;
        
        
        var toDelete = await db.OutboxMessages
            .Where(m => m.ProcessedAtUtc != null && m.ProcessedAtUtc <= cutoff)
            .Take(batchSize)
            .ToListAsync(ct);
        
        if (toDelete.Count == 0) return 0;
        
        db.OutboxMessages.RemoveRange(toDelete);
        await db.SaveChangesAsync(ct);
        
        
        return toDelete.Count;
    }
}