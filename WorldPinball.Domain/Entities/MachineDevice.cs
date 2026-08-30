namespace WorldPinball.Domain.Entities;

public class MachineDevice
{
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DateTime AttachedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DetachedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}