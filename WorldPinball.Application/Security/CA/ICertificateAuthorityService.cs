namespace WorldPinball.Application.Security.CA;

public interface ICertificateAuthorityService
{
    Task<IssuedCertificate> IssueDeviceCertificateAsync(
        Guid machineId,
        string deviceSerialNumber,
        string publicKeyPem,
        CancellationToken ct = default);
}
