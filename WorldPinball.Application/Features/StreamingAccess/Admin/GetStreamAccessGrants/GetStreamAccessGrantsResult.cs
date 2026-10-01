using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Dtos;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants;

public abstract record GetStreamAccessGrantsResult
{
    private GetStreamAccessGrantsResult() { }
    public sealed record Success(IReadOnlyList<StreamAccessGrantDto> Grants) : GetStreamAccessGrantsResult;
}