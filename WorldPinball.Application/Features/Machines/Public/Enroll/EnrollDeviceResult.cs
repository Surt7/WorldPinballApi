namespace WorldPinball.Application.Features.Machines.Public.Enroll;

public abstract record EnrollDeviceResult
{
    public sealed record Success(
        Guid DeviceId,
        Guid MachineId,
        string CertPem,
        string CertSerialNumber
    ) : EnrollDeviceResult;

    public sealed record InvalidToken(string Reason) : EnrollDeviceResult;
    public sealed record DeviceAlreadyEnrolled(string SerialPhysical) : EnrollDeviceResult;
    public sealed record MachineNotFound(Guid MachineId) : EnrollDeviceResult;
}