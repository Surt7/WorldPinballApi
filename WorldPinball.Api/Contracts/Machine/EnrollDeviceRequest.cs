namespace WorldPinball.Api.Contracts.Machine;

public sealed record EnrollDeviceRequest(
    Guid MachineId,
    string PlainEnrollmentToken,
    string PublicKeyPem,
    string DeviceSerialNumber
    );