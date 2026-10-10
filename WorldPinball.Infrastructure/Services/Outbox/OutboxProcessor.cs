using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Services.Outbox;


public interface IOutboxProcessor
{
    Task ProcessBatchAsync(CancellationToken ct = default);
}

public class OutboxProcessor(
    AppDbContext db,
    IEventPublisher publisher,
    TimeProvider timeProvider) : IOutboxProcessor
{
    public async Task ProcessBatchAsync(CancellationToken ct = default)
    {
        await using var tx = db.Database.IsRelational() 
            ? await db.Database.BeginTransactionAsync(ct) 
            : null;

        
   /* nota bene:
    transaction must be explicit and
    the query is not fluent or query syntax because
    we have to use postgres feature for concurrency
   */
        var messages = db.Database.IsRelational()
            ? await db.OutboxMessages
                .FromSql($"""
                          SELECT * 
                          FROM outbox_messages
                          WHERE processed_at_utc IS NULL AND retry_count < 5
                          ORDER BY occurred_at_utc
                           LIMIT 20
                            FOR UPDATE SKIP LOCKED
                          """)
                .ToListAsync(ct)
            
            : await db.OutboxMessages
                .Where(m => m.ProcessedAtUtc == null && m.RetryCount < 5)
                .OrderBy(m => m.OccurredAtUtc)
                .Take(20)
                .ToListAsync(ct);

        if (messages.Count == 0)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Process and publish each message
        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync("game-sessions", 
                    message.Id.ToString(),
                    message.Content,
                    ct);
                
                message.MarkAsProcessed(now);
            }
            catch (Exception ex)
            {
                // Kafka is down: increment retry count and keep message safe in PostgreSQL
                message.RecordFailure(ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
        if (tx is not null)
        {
            await tx.CommitAsync(ct);
        }
    }
    
   
}