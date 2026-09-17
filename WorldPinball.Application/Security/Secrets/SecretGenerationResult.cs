namespace WorldPinball.Application.Security.Secrets;

public sealed record SecretGenerationResult(
    string PlainBase64,
    string HashHex,
    string Encrypted
);
