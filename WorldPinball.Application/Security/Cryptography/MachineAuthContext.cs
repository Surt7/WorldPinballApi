namespace WorldPinball.Application.Security.Cryptography;

/// <summary>
/// Encapsulates the raw context extracted from a signed machine request.
/// Free of any ASP.NET Core Http dependencies for pure testability.
/// </summary>
public sealed record MachineAuthContext(
    string? DeviceIdHeader,
    string? TimestampHeader,
    string? NonceHeader,
    string? SignatureHeader,
    string HttpMethod,
    string RequestPath,
    string RawBody
);
