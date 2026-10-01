using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Queries;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants;

public interface IGetStreamAccessGrantsHandler
{
    Task<GetStreamAccessGrantsResult> HandleAsync(
        GetStreamAccessGrantsQuery query,
        CancellationToken cancellationToken = default);
}