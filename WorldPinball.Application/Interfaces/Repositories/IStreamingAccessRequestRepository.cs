using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IStreamingAccessRequestRepository
{
    Task<StreamingAccessRequest?> GetLatestRequestAsync(Guid sessionId, CancellationToken token = default);
    
    Task AddAsync(StreamingAccessRequest request, CancellationToken token = default);
    
    Task UpdateAsync(StreamingAccessRequest request, CancellationToken token = default);
}