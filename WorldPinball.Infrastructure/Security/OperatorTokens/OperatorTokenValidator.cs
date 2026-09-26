using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Security;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Security.OperatorTokens;

public sealed class OperatorTokenValidator(
    AppDbContext dbContext,
    IMemoryCache cache,
    TimeProvider timeProvider,
    ILogger<OperatorTokenValidator> logger) : IOperatorTokenValidator
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<OperatorToken?> ValidateAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length < 16)
        {
            return null;
        }

        var tokenHash = OperatorToken.HashToken(rawToken);
        var cacheKey = $"optok_{tokenHash}";

        // 1. Lookup in in-memory cache
        if (cache.TryGetValue(cacheKey, out OperatorToken? cachedToken) && cachedToken != null)
        {
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            if (cachedToken.IsValid(nowUtc))
            {
                return cachedToken;
            }

            cache.Remove(cacheKey);
            return null;
        }

        // 2. Query database for matching active token
        var token = await dbContext.OperatorTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (token == null)
        {
            logger.LogWarning("Operator token not found for hash prefix {HashPrefix}", tokenHash.Length >= 8 ? tokenHash[..8] : tokenHash);
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!token.IsValid(now))
        {
            logger.LogWarning("Operator token {TokenName} ({Prefix}) is inactive, expired, or revoked", token.Name, token.Prefix);
            return null;
        }

        // 3. Store valid token in memory cache
        cache.Set(cacheKey, token, CacheDuration);
        return token;
    }
}