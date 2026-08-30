namespace WorldPinball.Application.Security;

public sealed record SecretGenerationResult(
    string PlainBase64,
    string HashHex,
    string Encrypted
);