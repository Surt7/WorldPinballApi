using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class StreamAccessGrantRepository(AppDbContext context) : IStreamAccessGrantRepository
{
    public async Task<IReadOnlyList<StreamAccessGrant>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.StreamAccessGrants
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<StreamAccessGrant?> GetByIdAsync(Guid grantId, CancellationToken cancellationToken = default) =>
        await context.StreamAccessGrants
            .FirstOrDefaultAsync(g => g.GrantId == grantId, cancellationToken);

    public async Task<StreamAccessGrant?> FindActiveGrantAsync(
        PrincipalType principalType,
        Guid principalId,
        CancellationToken cancellationToken = default) =>
        await context.StreamAccessGrants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.PrincipalType == principalType &&
                     g.PrincipalId == principalId &&
                     g.Status == GrantStatus.Active,
                cancellationToken);

    public async Task AddAsync(StreamAccessGrant grant, CancellationToken cancellationToken = default)
    {
        await context.StreamAccessGrants.AddAsync(grant, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StreamAccessGrant grant, CancellationToken cancellationToken = default)
    {
        context.StreamAccessGrants.Update(grant);
        await context.SaveChangesAsync(cancellationToken);
    }
}