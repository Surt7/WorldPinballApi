using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Common;
using WorldPinball.Logistics.Domain.Entities;

namespace WorldPinball.Logistics.Infrastructure.Persistence.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor(
    TimeProvider timeProvider,
    ILogisticsOutboxChannel channel
) : SaveChangesInterceptor
{
    private readonly ConcurrentDictionary<DbContext, bool> _pendingNotifications = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
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
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                domainEvent.GetType().Name,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                timeProvider.GetUtcNow().UtcDateTime))
            .ToList();

        if (outboxMessages.Count > 0)
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
            _pendingNotifications[dbContext] = true;
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null &&
            _pendingNotifications.TryRemove(eventData.Context, out var hasMessages) &&
            hasMessages)
        {
            await channel.TryNotifyAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
    
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _pendingNotifications.TryRemove(eventData.Context, out _);
        }
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}
