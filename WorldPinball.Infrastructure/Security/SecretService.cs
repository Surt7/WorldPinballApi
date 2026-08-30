using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using WorldPinball.Application.Security;

namespace WorldPinball.Infrastructure.Security;

public class SecretService(IDataProtectionProvider provider) : ISecretService
{
    private readonly IDataProtector _protector = provider.CreateProtector("WorldPinball.MachineSecret.v1");

    public Application.Security.SecretGenerationResult Generate()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var secretBase64 = Convert.ToBase64String(secretBytes);
        var hashHex = Convert.ToHexString(SHA256.HashData(secretBytes)).ToLowerInvariant();
        var encrypted = _protector.Protect(secretBase64);

        var result = new Application.Security.SecretGenerationResult(
        secretBase64,
        hashHex,
        encrypted
    );


        return result;
    }
}
