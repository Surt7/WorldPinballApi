using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Api.Controllers.Public
{
    [Route("api/v1/ip-licenses")]
    [ApiController]
    public class IpLicensesController(IIpLicenseRepository ipLicenseRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var allIpLicenses = await ipLicenseRepository.GetAllAsync(cancellationToken);

            return Ok(allIpLicenses);
        }
    }
}
