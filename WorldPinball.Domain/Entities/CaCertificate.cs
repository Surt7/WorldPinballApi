using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class CaCertificate
{
    public Guid CaId { get; private set; }

    public CaType CaType { get; private set; }

    public string SubjectDn { get; private set; } = null!;
    public string IssuerDn { get; private set; } = null!;
    public string SerialNumber { get; private set; } = null!;
    public string ThumbprintSha256 { get; private set; } = null!;

    public string CertPem { get; private set; } = null!;

    // Reference to vault/HSM if applicable
    public string? PrivateKeyRef { get; private set; }

    public DateTime NotBeforeUtc { get; private set; }
    public DateTime NotAfterUtc { get; private set; }

    public bool IsTrusted { get; private set; } = true;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CaCertificate() { } // EF

    public CaCertificate(
        CaType caType,
        string subjectDn,
        string issuerDn,
        string serialNumber,
        string thumbprintSha256,
        string certPem,
        DateTime notBefore,
        DateTime notAfter,
        DateTime createdAt,
        bool isTrusted = true,
        bool isActive = true,
        string? privateKeyRef = null)
    {
        CaId = Guid.NewGuid();
        CaType = caType;
        SubjectDn = subjectDn ?? throw new ArgumentNullException(nameof(subjectDn));
        IssuerDn = issuerDn ?? throw new ArgumentNullException(nameof(issuerDn));
        SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
        ThumbprintSha256 = thumbprintSha256 ?? throw new ArgumentNullException(nameof(thumbprintSha256));
        CertPem = certPem ?? throw new ArgumentNullException(nameof(certPem));
        NotBeforeUtc = notBefore;
        NotAfterUtc = notAfter;
        CreatedAt = createdAt;
        IsTrusted = isTrusted;
        IsActive = isActive;
        PrivateKeyRef = privateKeyRef;
    }

    public void SetActive(bool active, DateTime now) { IsActive = active; UpdatedAt = now; }
    public void SetTrusted(bool trusted, DateTime now) { IsTrusted = trusted; UpdatedAt = now; }
}