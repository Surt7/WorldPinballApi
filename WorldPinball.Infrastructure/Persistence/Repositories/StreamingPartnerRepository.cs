using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class StreamingPartnerRepository(AppDbContext context) : IStreamingPartnerRepository
{
    public async Task<bool> IsActivePartnerAsync(Guid userId, CancellationToken token = default) =>
        await context.StreamingPartners
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.IsActive, token);
}