using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
            _pendingNotifications[dbContext] = true;
        }
        else
        {
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


    /*
     * Acts as a safety net when the database operation fails.
     * EF Core invokes this method instead of SavedChangesAsync.
     * Cleans up the tracking dictionary to prevent memory leaks and guards against null keys.
     * */
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            _pendingNotifications.TryRemove(eventData.Context, out _);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}