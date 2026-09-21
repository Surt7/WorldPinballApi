using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;
using WorldPinball.Application.Features.Leaderboard.Public.Queries;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.Leaderboard.Public.Handlers;

public sealed class GetLeaderboardHandler(
    ILeaderboardReadRepository repository,
    IRealtimeLeaderboardService realTimeService
)
{
public async Task<GetLeaderboardResult> HandleAsync(
        GetLeaderboardQuery query,
        CancellationToken ct = default)
    {
        var limit = Math.Clamp(query.Limit, 1, 200);
        var ttl = TimeSpan.FromMinutes(1);
        var hasGuids = query.IpId.HasValue && query.ModeId.HasValue;
        var hasSlugs = !string.IsNullOrWhiteSpace(query.IpSlug) 
                       && !string.IsNullOrWhiteSpace(query.ModeCode);
        
        
        if (!hasGuids && !hasSlugs)
        {
            return new GetLeaderboardResult.InvalidParameters(
                "Either (IpId and ModeId) or (IpSlug and ModeCode) must be provided.");
        }
        
        
        if (query.IpId is Guid ipId && query.ModeId is Guid modeId)
        {
            var cached = await realTimeService.GetCachedLeaderboardAsync(
                ipId, modeId, limit, ct);
            if (cached is not null)
            {
                return new GetLeaderboardResult.Success(cached);
            }
        }
        
        
        var dbResults = await repository.GetLeaderboardAsync(
            query.IpId, query.ModeId, query.IpSlug, query.ModeCode, limit, ct);
        if (query.IpId is Guid ip && query.ModeId is Guid mode)
        {
            await realTimeService.SetCachedLeaderboardAsync(
                ip, mode, limit, dbResults, ttl, ct);
        }

        return new GetLeaderboardResult.Success(dbResults);
    }
}