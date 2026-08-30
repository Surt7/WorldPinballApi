using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class CertificateRevocation
{
    public Guid RevocationId { get; private set; }

    public Guid DeviceCertId { get; private set; }

    public DateTime RevokedAtUtc { get; private set; }
    public RevocationReason Reason { get; private set; }
    public string RevokedByUserId { get; private set; } = null!;
    public string? Notes { get; private set; }

    // Navigation
    public DeviceCertificate DeviceCertificate { get; private set; } = null!;

    private CertificateRevocation() { } // EF

    public CertificateRevocation(Guid deviceCertId, DateTime revokedAt, RevocationReason reason, string revokedByUserId, string? notes = null)
    {
        RevocationId = Guid.NewGuid();
        DeviceCertId = deviceCertId;
        RevokedAtUtc = revokedAt;
        Reason = reason;
        RevokedByUserId = revokedByUserId ?? throw new ArgumentNullException(nameof(revokedByUserId));
        Notes = notes;
    }
}