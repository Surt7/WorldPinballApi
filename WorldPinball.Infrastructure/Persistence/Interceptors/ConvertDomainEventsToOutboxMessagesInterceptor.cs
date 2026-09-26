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
    private static readonly AsyncLocal<bool> HasNewMessages = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            HasNewMessages.Value = false;
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
            HasNewMessages.Value = true;
        }
        else
        {
            HasNewMessages.Value = false;
        }
            
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (HasNewMessages.Value)
        {
            HasNewMessages.Value = false;
            await channel.TryNotifyAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}