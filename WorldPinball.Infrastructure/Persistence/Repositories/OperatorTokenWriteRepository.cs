using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class OperatorTokenWriteRepository(AppDbContext db): IOperatorTokenWriteRepository
{
    public async Task AddAsync(OperatorToken token, CancellationToken ct = default)
    {
        await db.OperatorTokens.AddAsync(token, ct);
        await db.SaveChangesAsync(ct);
    }
}