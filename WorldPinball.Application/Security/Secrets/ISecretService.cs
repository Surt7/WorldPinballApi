using System.Diagnostics.CodeAnalysis;

namespace WorldPinball.Application.Security.Secrets;

public interface ISecretService
{
    SecretGenerationResult Generate();

    bool TryHash(string plainBase64Secret, [NotNullWhen(true)] out string? hashHex);
}