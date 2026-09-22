using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Security.Cryptography;

public sealed class MachineAuthService(
    AppDbContext db,
    ICanonicalBuilder canonicalBuilder,
    ISignatureVerifier signatureVerifier,
    TimeProvider timeProvider,
    ILogger<MachineAuthService> logger
)
    : IMachineAuthService

{
    private AppDbContext _db = db;
    private static readonly EventId InvalidHeaderReceived = new(3100, nameof(InvalidHeaderReceived));
    private static readonly EventId DeviceNotAttached = new(3102, nameof(DeviceNotAttached));
    private static readonly EventId DeviceRevokedOrInactive = new(3103, nameof(DeviceRevokedOrInactive));
    private static readonly EventId DeviceMissingPublicKey = new(3104, nameof(DeviceMissingPublicKey));
    private static readonly EventId SignatureVerificationFailed = new(3105, nameof(SignatureVerificationFailed));
    private static readonly EventId MachineAuthenticated = new(3106, nameof(MachineAuthenticated));

    private static class ErrorMessages
    {
        public const string InvalidDeviceId = "Invalid or missing GUID format.";
        public const string InvalidTimestamp = "Invalid or missing Unix timestamp.";
        public const string InvalidNonce = "Nonce is missing or too short (minimum 16 characters).";
        public const string MissingSignature = "Signature header is missing.";
        public const string TimestampOutOfWindow = "Request timestamp is outside the allowed tolerance window.";
        public const string DeviceNotFoundOrRevoked = "Device not found or revoked.";
        public const string DeviceNotAttached = "Device is not actively attached to a machine.";
        public const string MissingPublicKey = "Device public key is missing.";
        public const string InvalidSignature = "Cryptographic signature verification failed.";
        public const string UnsupportedAlgorithm = "Signature algorithm is not supported.";
    }

    private sealed record AttachedDevice(
        Guid MachineId,
        Guid DeviceId,
        DeviceStatus Status,
        string PublicKey,
        string KeyAlgorithm
    );

    private readonly record struct ParsedHeaders(
        Guid DeviceId,
        long Timestamp,
        string Nonce,
        string SignatureBase64
    );

    private static bool TryParseHeaders(
        MachineAuthContext context,
        out ParsedHeaders parsed,
        out MachineAuthResult.InvalidHeader? error)
    {
        parsed = default;
        error = null;

        if (!Guid.TryParse(context.DeviceIdHeader, out var deviceId))
        {
            error = new MachineAuthResult.InvalidHeader("X-Device-Id", ErrorMessages.InvalidDeviceId);
            return false;
        }

        if (!long.TryParse(context.TimestampHeader, out var timestamp))
        {
            error = new MachineAuthResult.InvalidHeader("X-Timestamp", ErrorMessages.InvalidTimestamp);
            return false;
        }

        if (string.IsNullOrWhiteSpace(context.NonceHeader) || context.NonceHeader.Length < 16)
        {
            error = new MachineAuthResult.InvalidHeader("X-Nonce", ErrorMessages.InvalidNonce);
            return false;
        }

        if (string.IsNullOrWhiteSpace(context.SignatureHeader))
        {
            error = new MachineAuthResult.InvalidHeader("X-Signature", ErrorMessages.MissingSignature);
            return false;
        }

        parsed = new ParsedHeaders(deviceId, timestamp, context.NonceHeader, context.SignatureHeader);
        return true;
    }


    public async Task<MachineAuthResult> ValidateAsync(MachineAuthContext context, CancellationToken ct = default)
    {
        if (!TryParseHeaders(context, out var headers, out var headerError))
        {
            logger.LogWarning(
                InvalidHeaderReceived,
                "Machine authentication failed: Header {HeaderName} is invalid ({Reason}).",
                headerError!.HeaderName,
                headerError.Reason);
            return headerError!;
        }

        // timestamp window (±120s) anti-replay
        var timestampError = ValidateTimestampWindow(headers.Timestamp);

        if (timestampError is not null)
        {
            return timestampError;
        }


        var attached = await GetActiveAttachedDevice(headers.DeviceId, ct);

        if (attached is null)
        {
            logger.LogWarning(
                DeviceNotAttached,
                "Machine authentication rejected: Device {DeviceId} is not actively attached to any machine.",
                headers.DeviceId);

            return new MachineAuthResult.DeviceNotAttached(headers.DeviceId);
        }

        if (attached.Status != DeviceStatus.Active)
        {
            logger.LogWarning(
                DeviceRevokedOrInactive,
                "Machine authentication rejected: Device {DeviceId} status is {Status} (expected Active).",
                headers.DeviceId,
                attached.Status);

            return new MachineAuthResult.DeviceNotFoundOrRevoked(headers.DeviceId);
        }

        if (string.IsNullOrWhiteSpace(attached.PublicKey))
        {
            logger.LogWarning(
                DeviceMissingPublicKey,
                "Machine authentication rejected: Device {DeviceId} has no registered public key.",
                headers.DeviceId);

            return new MachineAuthResult.MissingPublicKey(headers.DeviceId);
        }

        byte[] canonicalBytes = canonicalBuilder.Build(
            headers.Timestamp,
            headers.Nonce,
            context.HttpMethod,
            context.RequestPath,
            context.RawBody);

        bool isValidSignature;
        try
        {
            isValidSignature = signatureVerifier.Verify(attached.PublicKey, canonicalBytes, headers.SignatureBase64,
                attached.KeyAlgorithm);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                SignatureVerificationFailed,
                ex,
                "Machine authentication failed: Signature verification threw an exception for device {DeviceId}.",
                headers.DeviceId);

            isValidSignature = false;
        }

        if (!isValidSignature)
        {
            logger.LogWarning(
                SignatureVerificationFailed,
                "Machine authentication failed: Signature mismatch for device {DeviceId}.",
                headers.DeviceId);

            return new MachineAuthResult.InvalidSignature(ErrorMessages.InvalidSignature);
        }

        logger.LogInformation(
            MachineAuthenticated,
            "Machine {MachineId} successfully authenticated using device {DeviceId}.",
            attached.MachineId,
            headers.DeviceId);

        return new MachineAuthResult.Success(attached.MachineId, headers.DeviceId, headers.Nonce, headers.Timestamp);
    }

    private async Task<AttachedDevice?> GetActiveAttachedDevice(
        Guid deviceId,
        CancellationToken ct
    )
    {
        return await _db.MachineDevices
            .AsNoTracking()
            .Where(md => md.DeviceId == deviceId && md.IsActive)
            .Select(md => new AttachedDevice(
                md.MachineId,
                md.DeviceId,
                md.Device.Status,
                md.Device.PublicKey ?? string.Empty,
                md.Device.KeyAlgorithm ?? string.Empty
            )).FirstOrDefaultAsync(ct);
    }


    private MachineAuthResult? ValidateTimestampWindow(long requestTimestamp)
    {
        var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var diff = Math.Abs(now - requestTimestamp);

        if (diff > 120)
        {
            return new MachineAuthResult.TimestampOutOfWindow(diff);
        }

        return null;
    }
}