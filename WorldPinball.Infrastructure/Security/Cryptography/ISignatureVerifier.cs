namespace WorldPinball.Infrastructure.Security;

public interface ISignatureVerifier
{
    public bool Verify(string publicKeyPem, byte[] data, string signatureBase64, string? algorithm = null);
}