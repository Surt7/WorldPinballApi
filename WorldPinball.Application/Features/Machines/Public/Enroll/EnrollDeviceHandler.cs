using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Public.Enroll;
using WorldPinball.Application.Features.Machines.Public.Enroll.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.CA;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.Machines.Public;

public sealed class EnrollDeviceHandler(
    IDeviceEnrollmentWriteRepository deviceEnrollmentWriteRepository,
    IDeviceEnrollmentReadRepository deviceEnrollmentReadRepository,
    ICertificateAuthorityService caService,
    ILogger<EnrollDeviceHandler> logger,
    TimeProvider timeProvider
) : IEnrollDeviceHandler
{
    private static readonly EventId DeviceEnrolled =
        new(2100, nameof(DeviceEnrolled));

    private static readonly EventId EnrollmentTokenNotFound =
        new(2101, nameof(EnrollmentTokenNotFound));

    private static readonly EventId DeviceAlreadyEnrolled =
        new(2102, nameof(DeviceAlreadyEnrolled));

    private static readonly EventId EnrollmentTokenConsumptionFailed =
        new(2103, nameof(EnrollmentTokenConsumptionFailed));

    public async Task<EnrollDeviceResult> HandleAsync(
        EnrollDeviceCommand cmd,
        CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;


        string plainTextEnrollementToken = cmd.PlainEnrollmentToken;

        if (string.IsNullOrWhiteSpace(plainTextEnrollementToken))
            throw new ArgumentNullException(
                cmd.PlainEnrollmentToken,
                $"{nameof(cmd.PlainEnrollmentToken)}" +
                $" cannot be null or empty");
        
        Span<byte> secretBytes = stackalloc byte[32];
        if (!Convert.TryFromBase64String(plainTextEnrollementToken, secretBytes, out _))
        {
            return new EnrollDeviceResult.InvalidToken("Invalid token format");
        }
        
        Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(secretBytes, hashBytes);
        
        string tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        EnrollmentToken? token = await deviceEnrollmentReadRepository.GetTokenByHashAsync(tokenHash, ct);

        if (token is null)
        {
            logger.LogWarning(
                EnrollmentTokenNotFound,
                "Device enrollment rejected for serial {DeviceSerialNumber}: Token not found.",
                cmd.DeviceSerialNumber);
            return new EnrollDeviceResult.InvalidToken("Token not found");
        }

        if (await deviceEnrollmentReadRepository.DeviceSerialExistsAsync(cmd.DeviceSerialNumber, ct))
        {
            logger.LogWarning(
                DeviceAlreadyEnrolled,
                "Device enrollment rejected for serial {DeviceSerialNumber}: Device is already enrolled.",
                cmd.DeviceSerialNumber);
            return new EnrollDeviceResult.DeviceAlreadyEnrolled(cmd.DeviceSerialNumber);
        }

        var device = new Device
        {
            DeviceId = Guid.NewGuid(),
            SerialPhysical = cmd.DeviceSerialNumber,
            PublicKey = cmd.PublicKeyPem,
            CreatedAtUtc = now
        };

        var machineDevice = new MachineDevice
        {
            MachineId = cmd.MachineId,
            DeviceId = device.DeviceId,
            AttachedAtUtc = now,
            IsActive = true
        };

        try
        {
            token.Consume(device.DeviceId, now);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                EnrollmentTokenConsumptionFailed,
                "Device enrollment rejected for serial {DeviceSerialNumber}: Token consumption failed ({Reason}).",
                cmd.DeviceSerialNumber,
                ex.Message);
            return new EnrollDeviceResult.InvalidToken(ex.Message);
        }

        await deviceEnrollmentWriteRepository.SaveEnrollmentAsync(device, machineDevice, token, ct);

        
        
        var issuedCert = await caService.IssueDeviceCertificateAsync(
            cmd.DeviceSerialNumber,
            cmd.PublicKeyPem,
            ct);

        logger.LogInformation(
            DeviceEnrolled,
            "Device {DeviceId} with serial {DeviceSerialNumber} successfully enrolled with CertSerial {CertSerial}.",
            device.DeviceId,
            cmd.DeviceSerialNumber,
            issuedCert.SerialNumber);
        
        
        logger.LogInformation(
            DeviceEnrolled,
            "Device {DeviceId} with serial {DeviceSerialNumber} successfully enrolled and attached to machine {MachineId}.",
            device.DeviceId,
            cmd.DeviceSerialNumber,
            cmd.MachineId);

        return new EnrollDeviceResult.Success(
            DeviceId: device.DeviceId,
            MachineId: cmd.MachineId,
            CertPem: issuedCert.CertPem,
            CertSerialNumber: issuedCert.SerialNumber
        );
    }
}