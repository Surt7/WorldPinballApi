using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class StreamingAccessRequestRepository(AppDbContext context) : IStreamingAccessRequestRepository
{
    public async Task<StreamingAccessRequest?> GetLatestRequestAsync(Guid sessionId, CancellationToken token = default) =>
        await context.StreamingAccessRequests
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(r => r.SessionId == sessionId, token);

    public async Task AddAsync(StreamingAccessRequest request, CancellationToken token = default) =>
        await context.StreamingAccessRequests.AddAsync(request, token);

    public Task UpdateAsync(StreamingAccessRequest request, CancellationToken token = default)
    {
        context.StreamingAccessRequests.Update(request);
        return Task.CompletedTask;
    }
    
    public async Task<StreamingAccessRequest?> GetByIdAsync(Guid requestId, CancellationToken token = default) =>
        await context.StreamingAccessRequests.FirstOrDefaultAsync(r => r.RequestId == requestId, token);
}