using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Public.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.Machines.Public;

public sealed class EnrollDeviceHandler(
    IDeviceEnrollmentWriteRepository deviceEnrollmentWriteRepository,
    IDeviceEnrollmentReadRepository deviceEnrollmentReadRepository,
    ILogger<EnrollDeviceHandler> logger,
    TimeProvider timeProvider
) : IEnrollDeviceHandler
{
    private static readonly EventId DeviceEnrolled =
        new(2100, nameof(DeviceEnrolled));

    private static readonly EventId DeviceEnrollmentRejected =
        new(2101, nameof(DeviceEnrollmentRejected));

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

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextEnrollementToken));
        string tokenHash = Convert.ToHexString(hashBytes);

        EnrollmentToken? token = await deviceEnrollmentReadRepository.GetTokenByHashAsync(tokenHash, ct);

        if (token == null)
            return new EnrollDeviceResult.InvalidToken("Token not found");

        if (await deviceEnrollmentReadRepository.DeviceSerialExistsAsync(cmd.DeviceSerialNumber, ct))
            return new EnrollDeviceResult.DeviceAlreadyEnrolled(cmd.DeviceSerialNumber);


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
            return new EnrollDeviceResult.InvalidToken(ex.Message);
        }
        await deviceEnrollmentWriteRepository.SaveEnrollmentAsync(device, machineDevice, token, ct);

        return new EnrollDeviceResult.Success(
            DeviceId: device.DeviceId,
            MachineId: cmd.MachineId,
            CertPem: "DUMMY_CERT_PEM",
            CertSerialNumber: "DUMMY_CERT_SERIAL"
        );
    }
}