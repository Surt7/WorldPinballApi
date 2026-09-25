using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Common;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Interceptors;


public sealed class ConvertDomainEventsToOutboxMessagesInterceptor(
    TimeProvider timeProvider,
    IOutboxNotificationChannel channel
    ) 
    : SaveChangesInterceptor
{
    
 
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {

 
        await channel.TryNotifyAsync(cancellationToken);

        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        var outboxMessages = dbContext.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return domainEvents;
            }).Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                domainEvent.GetType().Name,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                timeProvider.GetUtcNow().UtcDateTime))
            .ToList();
            
        if (outboxMessages.Count > 0)
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }
            
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
  
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await channel.TryNotifyAsync(cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
    
}