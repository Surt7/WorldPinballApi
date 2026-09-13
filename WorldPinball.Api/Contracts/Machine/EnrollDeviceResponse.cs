namespace WorldPinball.Api.Contracts.Machine;

public sealed record EnrollDeviceResponse(
    Guid DeviceId,
    Guid MachineId,
    string CertPem,
    string CertSerialNumber
    );