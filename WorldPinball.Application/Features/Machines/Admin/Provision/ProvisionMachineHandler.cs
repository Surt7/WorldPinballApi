using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Admin.Provision;

public sealed class ProvisionMachineHandler(
    IIpLicenseReadRepository ipLicenseRepository,
    IMachineWriteRepository machineWriteRepository,
    ISecretService secretService,
    ILogger<ProvisionMachineHandler> logger,
    TimeProvider timeProvider
    ):IProvisionMachineHandler
{
    //event id for seq log
    private static readonly EventId MachineProvisioned =
        new(2000, nameof(MachineProvisioned));
    
    private static readonly EventId MachineProvisioningUnknownIp =
        new(2001, nameof(MachineProvisioningUnknownIp));

    private static readonly EventId MachineProvisioningDuplicateSerial = 
        new(2002, nameof(MachineProvisioningDuplicateSerial));
    
    private static readonly EventId MachineProvisioningSameIdempotencyKey = 
        new(2003, nameof(MachineProvisioningSameIdempotencyKey));
    
    public async Task<ProvisionMachineResult> HandleAsync(
        ProvisionMachineCommand cmd,
        CancellationToken ct = default)
    {
        // Check if Ip license exist
        var ipLicense = await ipLicenseRepository.GetActiveBySlugAsync(
            cmd.IpSlug,
            ct);

        if (string.IsNullOrWhiteSpace(ipLicense?.Name))
        {
            logger.LogWarning(
                MachineProvisioningUnknownIp,
                "Machine provisioning rejected: IP {IpSlug} was not found.",
                cmd.IpSlug);
            return new ProvisionMachineResult.UnknownIp(cmd.IpSlug);
        }

        
        bool sameIdempotencyKey = await machineWriteRepository.SameIdempotencyKeyExistsAsync(cmd.IdempotencyKey, ct);
        if (sameIdempotencyKey)
        {
            logger.LogWarning(
                MachineProvisioningSameIdempotencyKey,
                "Same Idempotency key {IdempotencyKey} already exists.",
                cmd.IdempotencyKey);
            return new ProvisionMachineResult.SameIdempotencyKey(cmd.IdempotencyKey);
        }
        
        
        // check if WorldPinballSerialNumber exist  ( it's an internal serial number to worldPinball)
        bool serialExists = await machineWriteRepository.SerialExistsAsync(
            cmd.WorldPinballSerialNumber,
            ct);

        if (serialExists)
        {
            logger.LogWarning(
                MachineProvisioningDuplicateSerial,
                "Machine provisioning rejected: serial number {SerialNumber} already exists.",
                cmd.WorldPinballSerialNumber);
            
            return new ProvisionMachineResult.DuplicateSerial(cmd.WorldPinballSerialNumber);
        }
        
        var enrollmentSecret = secretService.Generate();
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var idempotencyKey = cmd.IdempotencyKey;
        var bytes = RandomNumberGenerator.GetBytes(32);
        var nonce = WebEncoders.Base64UrlEncode(bytes);
        
        var machine = new Machine(
            ipLicense.IpId,
            cmd.WorldPinballSerialNumber,
            cmd.DisplayName,
            nowUtc,
            idempotencyKey,
            nonce
            );

        var enrollmentToken = new EnrollmentToken(
            tokenHash: enrollmentSecret.HashHex,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: cmd.IssuedByUserId,
            issuedAt: nowUtc,
            expiresAt: nowUtc.AddHours(24),
            machineId: machine.MachineId,
            deviceId: null,
            maxUses: 1);
        
        await machineWriteRepository.AddProvisioningAsync(
            machine,
            enrollmentToken,
            ct);
        
        logger.LogInformation(
            MachineProvisioned,
            "Machine {MachineId} provisioned for IP {IpId} ({IpSlug}) by {IssuedByUserId}.",
            machine.MachineId,
            machine.IpId,
            cmd.IpSlug,
            cmd.IssuedByUserId);
        
        
        return new ProvisionMachineResult.Success(
            machine.MachineId,
            enrollmentSecret.PlainBase64);
    }
}
