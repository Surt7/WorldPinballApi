namespace WorldPinball.Api.Filters.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotentResponse?> GetAsync(
        string key, CancellationToken ct = default);
    
    Task SetAsync(
        string key, 
        IdempotentResponse response, 
        TimeSpan duration, 
        CancellationToken ct = default);
}