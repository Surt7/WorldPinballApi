using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class Device
{
    public Guid DeviceId { get; set; }

    public string SerialPhysical { get; set; } = null!;
    public DeviceStatus Status { get; set; } = DeviceStatus.Pending;

    public string? PublicKey { get; set; }
    public string? PublicKeyFingerprint { get; set; }
    public string? KeyType { get; set; }
    public string? KeyAlgorithm { get; set; }

    public long LastCounter { get; set; } = 0;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProvisionedAtUtc { get; set; }
    public DateTime? ActivatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }

    public string? ClientVersion { get; set; }
    public string? Notes { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<MachineDevice> MachineDevices { get; private set; } = [];
    public ICollection<DeviceCertificate> Certificates { get; private set; } = [];
    public ICollection<EnrollmentToken> ConsumedTokens { get; private set; } = [];
}