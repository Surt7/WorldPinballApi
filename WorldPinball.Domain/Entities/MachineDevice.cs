namespace WorldPinball.Domain.Entities;

public class MachineDevice
{
    private static class ErrorMessages
    {
        public const string InvalidMachineId = "MachineId cannot be empty.";
        public const string InvalidDeviceId = "DeviceId cannot be empty.";
        public const string InvalidDate = "Date cannot be default.";
        public const string AlreadyDetached = "Device is already detached.";
    }

    private MachineDevice() { } // EF Core

    public Guid MachineId { get; private set; }
    public Machine Machine { get; private set; } = null!;

    public Guid DeviceId { get; private set; }
    public Device Device { get; private set; } = null!;

    public DateTime AttachedAtUtc { get; private set; }
    public DateTime? DetachedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public MachineDevice(
        Guid machineId,
        Guid deviceId,
        DateTime attachedAtUtc)
    {
        if (machineId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.InvalidMachineId, nameof(machineId));

        if (deviceId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.InvalidDeviceId, nameof(deviceId));

        if (attachedAtUtc == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(attachedAtUtc));

        MachineId = machineId;
        DeviceId = deviceId;
        AttachedAtUtc = attachedAtUtc;
        IsActive = true;
    }

    public void Detach(DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        if (!IsActive)
            throw new InvalidOperationException(ErrorMessages.AlreadyDetached);

        IsActive = false;
        DetachedAtUtc = now;
    }
}
