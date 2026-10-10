using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses.Queries;

namespace WorldPinball.Api.Controllers.Public;

[Route("api/v1/ip-licenses")]
[ApiController]
public class IpLicensesController(
    IGetAllIpLicensesHandler getAllIpLicensesHandler
) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Returns all available IP licenses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await getAllIpLicensesHandler.HandleAsync(new GetAllIpLicensesQuery(), ct);

        return result switch
        {
            GetAllIpLicensesResult.Success success => Ok(success.IpLicenses),
            GetAllIpLicensesResult.NotFound => NotFound(new
            {
                message = "No IP licenses found."
            }),
            _ => throw new InvalidOperationException($"Unsupported result: {result.GetType().Name}")
        };
    }
}
