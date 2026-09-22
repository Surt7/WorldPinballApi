
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Public.Enroll.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.CA;
using WorldPinball.Domain.Entities;
using WorldPinball.Application.Security.Secrets;

namespace WorldPinball.Application.Features.Machines.Public;

public sealed class EnrollDeviceHandler(
    IDeviceEnrollmentWriteRepository deviceEnrollmentWriteRepository,
    IDeviceEnrollmentReadRepository deviceEnrollmentReadRepository,
    ICertificateAuthorityService caService,
    ISecretService secretService,
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
    
    private static readonly EventId EnrollmentTokenIsInvalid =
        new(2104, nameof(EnrollmentTokenIsInvalid));

    private static readonly EventId MachineMismatch =
        new(2105, nameof(MachineMismatch));
    
    public async Task<EnrollDeviceResult> HandleAsync(
        EnrollDeviceCommand cmd,
        CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        string plainTextEnrollementToken = cmd.PlainEnrollmentToken;

        if (cmd.MachineId == Guid.Empty)
        {
            logger.LogWarning(
                MachineMismatch,
                "Device enrollment rejected for serial {DeviceSerialNumber}: MachineId is empty.",
                cmd.DeviceSerialNumber);
            return new EnrollDeviceResult.MachineNotFound(cmd.MachineId);
        }
        
        if (string.IsNullOrWhiteSpace(plainTextEnrollementToken))
            throw new ArgumentNullException(
                cmd.PlainEnrollmentToken,
                $"{nameof(cmd.PlainEnrollmentToken)}" +
                $" cannot be null or empty");
        
        if (!secretService.TryHash(plainTextEnrollementToken, out var tokenHash))
        {
            logger.LogWarning(
                EnrollmentTokenIsInvalid,
                "Device enrollment rejected for serial {DeviceSerialNumber}: Invalid token format.",
                cmd.DeviceSerialNumber);
            return new EnrollDeviceResult.InvalidToken("Invalid token format");
        }

        EnrollmentToken? token = await deviceEnrollmentReadRepository.GetTokenByHashAsync(tokenHash, ct);

        if (token is null)
        {
            logger.LogWarning(
                EnrollmentTokenNotFound,
                "Device enrollment rejected for serial {DeviceSerialNumber}: Token not found.",
                cmd.DeviceSerialNumber);
            return new EnrollDeviceResult.InvalidToken("Token not found");
        }


        if (token.MachineId != cmd.MachineId)
        {
            logger.LogWarning(
                MachineMismatch,
                "Device enrollment rejected: MachineId mismatch. Command authorized for {TokenMachineId} but requested {CommandMachineId}.",
                token.MachineId,
                cmd.MachineId);
            
            return new EnrollDeviceResult.MachineNotFound(cmd.MachineId);
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

        var machineDevice = new MachineDevice(
            machineId: cmd.MachineId,
            deviceId: device.DeviceId,
            attachedAtUtc: now
        );

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
            cmd.MachineId,
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
