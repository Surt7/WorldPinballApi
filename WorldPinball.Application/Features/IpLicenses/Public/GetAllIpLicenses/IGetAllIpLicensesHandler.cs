using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses.Queries;

namespace WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;

public interface IGetAllIpLicensesHandler
{
    Task<GetAllIpLicensesResult> HandleAsync(
        GetAllIpLicensesQuery query,
        CancellationToken cancellationToken = default);
}
