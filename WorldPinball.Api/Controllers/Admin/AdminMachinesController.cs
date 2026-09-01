//using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Application.Security;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Persistence;


namespace WorldPinball.Api.Controllers.Admin;


[ApiController]
[Route("api/v1/admin/machines")]
public class AdminMachinesController : ControllerBase
{
    private readonly AppDbContext _db;

    private readonly IIpLicenseRepository _ipLicenseRepository;

    private readonly ISecretService _secretService;

    public AdminMachinesController(
        AppDbContext db,
        IIpLicenseRepository ipLicenseRepository,
        ISecretService secretService)
    {
        _db = db;
        _ipLicenseRepository = ipLicenseRepository;
        _secretService = secretService;
    }

    [HttpPost("provision")]
    public async Task<ActionResult<ProvisionMachineResponse>> Provision([FromBody] ProvisionMachineRequest req)
    {
        
        
        var displayName = req.DisplayName;
        
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "The display name cannot be empty.",
                nameof(displayName));
        }
            
        // 1) check IP  (Intellectual Property / licence ex : Resident Evil)
        var ip = await _ipLicenseRepository.GetActiveBySlugAsync(req.IpSlug, HttpContext.RequestAborted);
        if (ip is null)
            return NotFound(new { message = "ipSlug inconnu/inactif" });
        

        // 2) serial unique
        var exists = await _db.Machines.AnyAsync(x => x.WorldPinballSerialNumber == req.WorldPinballSerialNumber);
        if (exists)
            return Conflict(new { message = "serial_number déjà existant" });
       


   
       

        _db.Machines.Add(machine);
        _db.EnrollmentTokens.Add(enrollmentToken);

        await _db.SaveChangesAsync();

   
        return Ok(new ProvisionMachineResponse(
            machine.MachineId,
            enrollmentSecret.PlainBase64
            ));
    }
}
