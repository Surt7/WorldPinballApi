//using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Features.Machines.Admin.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WorldPinball.Api.Controllers.Admin;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v1/admin/machines")]
public sealed class AdminMachinesController(
    IProvisionMachineHandler handler,
    IValidator<ProvisionMachineRequest> requestValidator) 
    : ControllerBase
{

    
    [HttpPost("provision")]
    [EndpointSummary("Creates a new machine and generates an enrollment token")]
    [EndpointDescription("POST to create a new machine.  Accepts a ProvisionMachineRequest.")]
    [ProducesResponseType(
        typeof(ProvisionMachineResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProvisionMachineResponse>> Provision(
        [FromBody] ProvisionMachineRequest req,
        CancellationToken ct)
    {
        var issuedByUser = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(issuedByUser))
        {
            return Unauthorized(new
            {
                message = "The authenticated user has no subject identifier."
            });
        }
        
        var validationResult = await requestValidator.ValidateAsync(req, ct);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
            ModelState.AddModelError(
                failure.PropertyName,
                failure.ErrorMessage);
            }
            
            return ValidationProblem(ModelState);
        }
        
        var displayName = req.DisplayName;
        var serialNumber = req.WorldPinballSerialNumber;
        var ip = req.IpSlug;

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
                    message = $"Unknow or inactive Ip slug: {unknownIp.IpSlug}"
                }),

            ProvisionMachineResult.DuplicateSerial duplicateSerial =>
                Conflict(new
                {
                    message = $"Already existing serial number: {duplicateSerial.SerialNumber}"
                }),

            _ => throw new InvalidOperationException(
                $"Unsupported provisioning result: {resultProvisioning.GetType().Name}")
        };
    }
    
    
    
}
