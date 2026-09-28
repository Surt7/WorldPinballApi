using WorldPinball.Application.Features.IpLicenses.Public.Dtos;

namespace WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;

public abstract record GetAllIpLicensesResult
{
    private GetAllIpLicensesResult() { }
    public sealed record Success(IReadOnlyList<IpLicenseDto> IpLicenses) : GetAllIpLicensesResult;
    public sealed record NotFound : GetAllIpLicensesResult;
}
