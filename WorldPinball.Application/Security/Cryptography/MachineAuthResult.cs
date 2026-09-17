namespace WorldPinball.Application.Security.Cryptography;

/// <summary>
/// Discriminated union representing all possible outcomes of machine request authentication.
/// </summary>
public abstract record MachineAuthResult
{
    private MachineAuthResult() { }

    
    private MachineAuthResult.TimestampOutOfWindow? ValidateTimestampWindow(long requestTimestamp)
    {
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var diff = Math.Abs(now - requestTimestamp);

        if (diff > 120)
        {
            return new MachineAuthResult.TimestampOutOfWindow(diff);
        }

        return null; // null signifie que tout est dans la fenêtre valide
    }
    
    public sealed record Success(
        Guid MachineId,
        Guid DeviceId,
        string Nonce,
        long Timestamp
    ) : MachineAuthResult;

    public sealed record InvalidHeader(string HeaderName, string Reason) : MachineAuthResult;

    public sealed record TimestampOutOfWindow(long DifferenceSeconds) : MachineAuthResult;

    public sealed record DeviceNotFoundOrRevoked(Guid DeviceId) : MachineAuthResult;

    public sealed record DeviceNotAttached(Guid DeviceId) : MachineAuthResult;

    public sealed record MissingPublicKey(Guid DeviceId) : MachineAuthResult;

    public sealed record InvalidSignature(string Reason) : MachineAuthResult;
}
