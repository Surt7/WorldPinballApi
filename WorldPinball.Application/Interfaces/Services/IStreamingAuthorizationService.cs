namespace WorldPinball.Application.Interfaces.Services;

public interface IStreamingAuthorizationService
{
    Task<bool> CanStreamAsync(Guid userId, Guid sessionId, 
        CancellationToken token = default);
}