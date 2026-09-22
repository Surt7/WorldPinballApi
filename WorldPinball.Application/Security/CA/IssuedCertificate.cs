namespace WorldPinball.Application.Security.CA;

public sealed record IssuedCertificate(
    string CertPem,
    string SerialNumber,
    Guid MachineId,
    string ThumbprintSha256,
    string SubjectDn,
    string IssuerDn,
    DateTime NotBeforeUtc,
    DateTime NotAfterUtc
);