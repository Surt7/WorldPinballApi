using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;

public sealed class GetAllIpLicensesHandler(
    IIpLicenseReadRepository ipLicenseRepository,
    ILogger<GetAllIpLicensesHandler> logger
) : IGetAllIpLicensesHandler
{
    private static readonly EventId FetchingIpLicenses = new(3300, nameof(FetchingIpLicenses));
    private static readonly EventId NoIpLicensesFound = new(3301, nameof(NoIpLicensesFound));

    public async Task<GetAllIpLicensesResult> HandleAsync(
        GetAllIpLicensesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(FetchingIpLicenses, "Fetching all IP licenses.");
        var ipLicenses = await ipLicenseRepository.GetAllAsync(cancellationToken);

        if (ipLicenses.Count == 0)
        {
            logger.LogWarning(NoIpLicensesFound, "No IP licenses found in the system.");
            return new GetAllIpLicensesResult.NotFound();
        }

        return new GetAllIpLicensesResult.Success(ipLicenses);
    }
}
