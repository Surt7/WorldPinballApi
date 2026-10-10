using System.Collections.Concurrent;

namespace WorldPinball.Api.Filters.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, (IdempotentResponse Response, DateTime ExpiresAt)> _cache = new();

    public Task<IdempotentResponse?> GetAsync(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow <= entry.ExpiresAt)
            {
                return Task.FromResult<IdempotentResponse?>(entry.Response);
            }

            _cache.TryRemove(key, out _);
        }

        return Task.FromResult<IdempotentResponse?>(null);
    }

    public Task SetAsync(string key, IdempotentResponse response, TimeSpan duration, CancellationToken ct = default)
    {
        var expiresAt = DateTime.UtcNow.Add(duration);
        _cache[key] = (response, expiresAt);
        return Task.CompletedTask;
    }
}
