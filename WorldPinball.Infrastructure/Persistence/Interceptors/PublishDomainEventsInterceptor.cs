using Microsoft.EntityFrameworkCore.Diagnostics;
using WorldPinball.Domain.Common;

namespace WorldPinball.Infrastructure.Persistence.Interceptors;

public sealed class PublishDomainEventsInterceptor:SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken  = default
        )
    {

        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entities = eventData.Context.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(entry => entry.DomainEvents)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}