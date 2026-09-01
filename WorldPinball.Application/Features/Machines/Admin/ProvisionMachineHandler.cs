using WorldPinball.Application.Features.Machines.Admin.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Admin;

public sealed class ProvisionMachineHandler(
    IIpLicenseRepository ipLicenseRepository,
    IMachineWriteRepository machineWriteRepository,
    ISecretService secretService)
{
    
    public async Task<ProvisionMachineResult> HandleAsync(
        ProvisionMachineCommand cmd,
        CancellationToken ct = default)
    {
        // Check if Ip license exist
        var ipLicense = await ipLicenseRepository.GetActiveBySlugAsync(
            cmd.IpSlug,
            ct);

        if (ipLicense is null)
            return new ProvisionMachineResult.UnknownIp(cmd.IpSlug);
        
        // check if WorldPinballSerialNumber exist  ( it's a internal serial number to worldPinball)
        var serialExists = await machineWriteRepository.SerialExistsAsync(
            cmd.WorldPinballSerialNumber,
            ct);

        if (serialExists)
            return new ProvisionMachineResult.DuplicateSerial(cmd.WorldPinballSerialNumber);
        
        var enrollmentSecret = secretService.Generate();

        var machine = new Machine(
            ipLicense.IpId,
            cmd.WorldPinballSerialNumber,
            cmd.DisplayName);

        var enrollmentToken = new EnrollmentToken(
            tokenHash: enrollmentSecret.HashHex,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: cmd.IssuedByUserId,
            issuedAt: machine.RegisteredAtUtc,
            expiresAt: machine.RegisteredAtUtc.AddHours(24),
            machineId: machine.MachineId,
            deviceId: null,
            maxUses: 1);
        
        await machineWriteRepository.AddProvisioningAsync(
            machine,
            enrollmentToken,
            ct);
        
        return new ProvisionMachineResult.Success(
            machine.MachineId,
            enrollmentSecret.PlainBase64);
    }
}
