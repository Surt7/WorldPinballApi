namespace WorldPinball.Application.Features.Machines.Public.Commands;

public sealed record EnrollDeviceCommand(
    Guid MachineId,
    string PlainEnrollmentToken,
    string PublicKeyPem,
    string DeviceSerialNumber
);