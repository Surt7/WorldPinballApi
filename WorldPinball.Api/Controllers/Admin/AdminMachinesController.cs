//using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Features.Machines.Admin.Commands;


namespace WorldPinball.Api.Controllers.Admin;


[ApiController]
[Route("api/v1/admin/machines")]
public sealed class AdminMachinesController(
    ProvisionMachineHandler handler) : ControllerBase
{

    [HttpPost("provision")]
    public async Task<ActionResult<ProvisionMachineResponse>> Provision(
        [FromBody] ProvisionMachineRequest req,
        CancellationToken ct)
    {
        
        
        var displayName = req.DisplayName;
        var serialNumber = req.WorldPinballSerialNumber;
        var ip = req.IpSlug;
        var issuedByUser = "Administrator-PM-01";
        
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("The display name cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(serialNumber)) throw new ArgumentException("The Serial Number cannot be empty.", nameof(serialNumber));
        if (string.IsNullOrWhiteSpace(ip)) throw new ArgumentException("The Ip name cannot be empty.", nameof(ip));
        if (string.IsNullOrWhiteSpace(issuedByUser)) throw new ArgumentException("the issuer cannot be empty.", nameof(issuedByUser));
        
        var cmd = new ProvisionMachineCommand(
            serialNumber,
            ip,
            displayName,
            issuedByUser
            );

        var resultProvisioning = await handler.HandleAsync(cmd, ct);

        return resultProvisioning switch
        {
            ProvisionMachineResult.Success success =>
                Ok(new ProvisionMachineResponse(
                    success.MachineId,
                    success.EnrollmentToken)),

            ProvisionMachineResult.UnknownIp unknownIp =>
                NotFound(new
                {
                    message = $"ipSlug inconnu/inactif: {unknownIp.IpSlug}"
                }),

            ProvisionMachineResult.DuplicateSerial duplicateSerial =>
                Conflict(new
                {
                    message = $"serial_number déjà existant: {duplicateSerial.SerialNumber}"
                }),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
