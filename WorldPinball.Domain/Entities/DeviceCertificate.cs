using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class DeviceCertificate
{
    public Guid DeviceCertId { get; private set; }

    public Guid DeviceId { get; private set; }
    public Guid CaId { get; private set; }

    public string CertSerialNumber { get; private set; } = null!;
    public string CertThumbprintSha256 { get; private set; } = null!;
    public string SubjectDn { get; private set; } = null!;
    public string IssuerDn { get; private set; } = null!;

    public string CertPem { get; private set; } = null!;

    public DateTime NotBeforeUtc { get; private set; }
    public DateTime NotAfterUtc { get; private set; }

    public DeviceCertificateStatus Status { get; private set; } = DeviceCertificateStatus.Active;
    public DateTime IssuedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokeReason { get; private set; }

    public Guid? SupersededByDeviceCertId { get; private set; }

    // Navigations
    public Device Device { get; private set; } = null!;
    public CaCertificate CaCertificate { get; private set; } = null!;
    public DeviceCertificate? SupersededBy { get; private set; }
    public CertificateRevocation? Revocation { get; private set; }

    private DeviceCertificate() { } // EF

    public DeviceCertificate(
        Guid deviceId,
        Guid caId,
        string certSerialNumber,
        string certThumbprintSha256,
        string subjectDn,
        string issuerDn,
        string certPem,
        DateTime notBeforeUtc,
        DateTime notAfterUtc,
        DateTime issuedAtUtc)
    {
        DeviceCertId = Guid.NewGuid();
        DeviceId = deviceId;
        CaId = caId;
        CertSerialNumber = certSerialNumber ?? throw new ArgumentNullException(nameof(certSerialNumber));
        CertThumbprintSha256 = certThumbprintSha256 ?? throw new ArgumentNullException(nameof(certThumbprintSha256));
        SubjectDn = subjectDn ?? throw new ArgumentNullException(nameof(subjectDn));
        IssuerDn = issuerDn ?? throw new ArgumentNullException(nameof(issuerDn));
        CertPem = certPem ?? throw new ArgumentNullException(nameof(certPem));
        NotBeforeUtc = notBeforeUtc;
        NotAfterUtc = notAfterUtc;
        IssuedAtUtc = issuedAtUtc;
        Status = DeviceCertificateStatus.Active;
    }

    public void MarkSuperseded(Guid newCertId)
    {
        Status = DeviceCertificateStatus.Superseded;
        SupersededByDeviceCertId = newCertId;
    }

    public void MarkRevoked(DateTime now, string reason)
    {
        Status = DeviceCertificateStatus.Revoked;
        RevokedAtUtc = now;
        RevokeReason = reason;
    }
}