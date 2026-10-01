using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Dtos;
using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants;

public sealed class GetStreamAccessGrantsHandler(
    IStreamAccessGrantRepository repository,
    ILogger<GetStreamAccessGrantsHandler> logger
) : IGetStreamAccessGrantsHandler
{
    private static readonly EventId FetchingGrants = new(3400, nameof(FetchingGrants));
    private static readonly EventId GrantsFetched = new(3401, nameof(GrantsFetched));

    public async Task<GetStreamAccessGrantsResult> HandleAsync(
        GetStreamAccessGrantsQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(FetchingGrants, "Fetching all stream access grants for admin.");

        var grants = await repository.GetAllAsync(cancellationToken);

        var dtos = grants.Select(g => new StreamAccessGrantDto(
            g.GrantId,
            g.PrincipalType.ToString(),
            g.PrincipalId,
            g.DisplayName,
            g.Origin.ToString(),
            g.Status.ToString(),
            g.CreatedAtUtc,
            g.ExpiresAtUtc,
            g.RevokedAtUtc,
            g.RevocationReason
        )).ToList();

        logger.LogInformation(GrantsFetched, "Successfully fetched {Count} stream access grants.", dtos.Count);

        return new GetStreamAccessGrantsResult.Success(dtos);
    }
}