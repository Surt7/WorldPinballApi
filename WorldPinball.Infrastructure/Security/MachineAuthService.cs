using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Security;

public sealed class MachineAuthResult
{
    public required Guid MachineId { get; init; }
    public required string Nonce { get; init; }
    public required long Timestamp { get; init; }
}

public interface IMachineAuthService
{
    Task<MachineAuthResult> ValidateAsync(HttpRequest req, string rawBody, CancellationToken ct);
}

public sealed class MachineAuthService(
    AppDbContext db,
    IDataProtectionProvider dp,
    ICanonicalBuilder canonicalBuilder,
    ISignatureVerifier signatureVerifier)
    : IMachineAuthService
{
    private readonly IDataProtector _protector = dp.CreateProtector("WorldPinball.MachineSecret.v1");

    public async Task<MachineAuthResult> ValidateAsync(HttpRequest req, string rawBody, CancellationToken ct)
    {
        var deviceIdStr = req.Headers["X-Device-Id"].ToString();
        var tsStr = req.Headers["X-Timestamp"].ToString();
        var nonce = req.Headers["X-Nonce"].ToString();
        var sigBase64 = req.Headers["X-Signature"].ToString();

        if (!Guid.TryParse(deviceIdStr, out var deviceId))
            throw new UnauthorizedAccessException("X-Machine-Id invalide");

        if (!long.TryParse(tsStr, out var ts))
            throw new UnauthorizedAccessException("X-Timestamp invalide");

        if (string.IsNullOrWhiteSpace(nonce) || nonce.Length < 16)
            throw new UnauthorizedAccessException("X-Nonce invalide");




        if (string.IsNullOrWhiteSpace(sigBase64))
            throw new UnauthorizedAccessException("X-Signature manquant");

        // timestamp window (±120s)
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - ts) > 120)
            throw new UnauthorizedAccessException("Timestamp hors fenêtre");

        // load device(machine in authority logic) + machine
        var device = await db.Devices
            .AsNoTracking()
            .Where(d => d.DeviceId == deviceId && d.Status == DeviceStatus.Active)
    .Select(d => new
    {
        d.DeviceId,
        d.PublicKey,
        d.PublicKeyFingerprint,
        d.KeyAlgorithm,
        d.KeyType,
        d.Status,
        d.LastCounter
    })
    .FirstOrDefaultAsync(ct);


        if (device is null || device.Status != DeviceStatus.Active)
            throw new UnauthorizedAccessException("Machine inconnue ou révoquée");

        var attached = await db.MachineDevices
    .AsNoTracking()
    .Where(md => md.DeviceId == deviceId && md.IsActive)
    .Select(md => new
    {
        md.MachineId,
        Status = md.Device.Status,
        md.Device.LastCounter,
        md.Device.PublicKey,
        md.Device.KeyAlgorithm,
        md.Device.KeyType
    })
    .FirstOrDefaultAsync(ct) ?? throw new UnauthorizedAccessException("Device non attaché");


        if (attached is not { Status: DeviceStatus.Active })
            throw new UnauthorizedAccessException("Device révoqué");

        if (string.IsNullOrWhiteSpace(attached.PublicKey))
            throw new UnauthorizedAccessException("Clé publique absente");

        byte[] canonicalBytes = canonicalBuilder.Build(ts, nonce, req.Method, req.Path.Value ?? "", rawBody);
        bool ok;
        try
        {
            ok = signatureVerifier.Verify(attached.PublicKey, canonicalBytes, sigBase64, attached.KeyAlgorithm);
        }
        catch (UnauthorizedAccessException) // garde les erreurs “format / algo / key”
        {
            throw;
        }
        catch
        {
            throw new UnauthorizedAccessException("Signature invalide");
        }

        if (!ok)
            throw new UnauthorizedAccessException("Signature invalide");





        Console.WriteLine("SIG_IN=" + sigBase64);







        return new MachineAuthResult { MachineId = deviceId, Nonce = nonce, Timestamp = ts };
    }
}
