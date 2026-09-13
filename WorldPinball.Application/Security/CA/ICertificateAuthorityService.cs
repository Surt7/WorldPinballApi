namespace WorldPinball.Application.Security.CA;

public interface ICertificateAuthorityService
{
    Task<IssuedCertificate> IssueDeviceCertificateAsync(
        string deviceSerialNumber,
        string publicKeyPem,
        CancellationToken ct = default);
}
