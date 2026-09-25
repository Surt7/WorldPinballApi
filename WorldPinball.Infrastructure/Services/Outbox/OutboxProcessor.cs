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
        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredAtUtc)
            .Take(20)
            .ToListAsync(ct);
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        
        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync("game-sessions", message.Id.ToString(), message.Content, ct);
                message.MarkAsProcessed(now);
            }
            catch (Exception ex)
            {
                message.RecordFailure(ex.Message);
            }
        }
        await db.SaveChangesAsync(ct);
    }
    
    
   
}