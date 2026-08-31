//using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Abstractions.Repositories;
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
        // 1) check IP
        var ip = await _ipLicenseRepository.GetActiveBySlugAsync(req.IpSlug, HttpContext.RequestAborted);
        if (ip is null)
            return NotFound(new { message = "ipSlug inconnu/inactif" });

        // 2) serial unique
        var exists = await _db.Machines.AnyAsync(x => x.WorldPinballSerialNumber == req.WorldPinballSerialNumber);
        if (exists)
            return Conflict(new { message = "serial_number déjà existant" });
        SecretGenerationResult enrollmentSecret = _secretService.Generate();
        // Important : on génère le token AVANT de créer la machine, car le token a besoin du MachineId pour être créé (lié à la machine)

        var now = DateTime.UtcNow;
        string serialNumber = req.WorldPinballSerialNumber;

        var machine = new Machine(ip.IpId, serialNumber)
        {
            Name = req.Name,

        };

        var enrollmentToken = new EnrollmentToken(
          tokenHash: enrollmentSecret.HashHex,
          tokenType: EnrollmentTokenType.DeviceEnroll,
          issuedByUserId: "917cf557-587d-4827-999c-35887b544c31",
          issuedAt: now,
          expiresAt: now.AddHours(24),
          machineId: machine.MachineId,
          deviceId: null,
          maxUses: 1
      );

        _db.Machines.Add(machine);
        _db.EnrollmentTokens.Add(enrollmentToken);

        await _db.SaveChangesAsync();

        // Retourne le secret UNE SEULE FOIS (tu l’injectes côté machine)
        return Ok(new ProvisionMachineResponse(
            machine.MachineId,
            enrollmentSecret.PlainBase64
            ));
    }
}
