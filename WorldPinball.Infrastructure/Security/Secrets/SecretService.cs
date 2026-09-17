using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using WorldPinball.Application.Security.Secrets;

namespace WorldPinball.Infrastructure.Security.Secrets;

public class SecretService(IDataProtectionProvider provider) : ISecretService
{
    private readonly IDataProtector _protector = provider.CreateProtector("WorldPinball.MachineSecret.v1");

    public SecretGenerationResult Generate()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var secretBase64 = Convert.ToBase64String(secretBytes);
        var hashHex = Convert.ToHexString(SHA256.HashData(secretBytes)).ToLowerInvariant();
        var encrypted = _protector.Protect(secretBase64);

        var result = new SecretGenerationResult(
            secretBase64,
            hashHex,
            encrypted
        );

        return result;
    }

    public bool TryHash(string plainBase64Secret, [NotNullWhen(true)] out string? hashHex)
    {
        hashHex = null;

        if (string.IsNullOrWhiteSpace(plainBase64Secret))
            return false;

        Span<byte> secretBytes = stackalloc byte[32];
        if (!Convert.TryFromBase64String(plainBase64Secret, secretBytes, out int bytesWritten)
            || bytesWritten != 32)
        {
            return false;
        }

        Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(secretBytes, hashBytes);

        hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return true;
    }
}
