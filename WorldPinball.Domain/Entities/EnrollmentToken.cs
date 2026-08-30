using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class EnrollmentToken
{
    public Guid TokenId { get; private set; }

    // Hash only (never store raw token)
    public string TokenHash { get; private set; } = null!;
    public EnrollmentTokenType TokenType { get; private set; } = EnrollmentTokenType.DeviceEnroll;

    // Optional scoping
    public Guid? MachineId { get; private set; }
    public Guid? DeviceId { get; private set; }

    public string IssuedByUserId { get; private set; } = null!;

    public DateTime IssuedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }
    public Guid? ConsumedByDeviceId { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public int MaxUses { get; private set; } = 1;
    public int UsesCount { get; private set; } = 0;

    public string? Notes { get; private set; }

    // Navigations (optional but useful)
    public Machine? Machine { get; private set; }
    public Device? Device { get; private set; }
    public Device? ConsumedByDevice { get; private set; }

    private EnrollmentToken() { } // EF

    public EnrollmentToken(
        string tokenHash,
        EnrollmentTokenType tokenType,
        string issuedByUserId,
        DateTime issuedAt,
        DateTime expiresAt,
        Guid? machineId = null,
        Guid? deviceId = null,
        int maxUses = 1,
        string? notes = null)
    {
        TokenId = Guid.NewGuid();
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        TokenType = tokenType;
        IssuedByUserId = issuedByUserId ?? throw new ArgumentNullException(nameof(issuedByUserId));
        IssuedAtUtc = issuedAt;
        ExpiresAtUtc = expiresAt;
        MachineId = machineId;
        DeviceId = deviceId;
        MaxUses = maxUses <= 0 ? 1 : maxUses;
        Notes = notes;
    }

    public void Consume(Guid deviceId, DateTime now)
    {
        if (RevokedAt is not null) throw new InvalidOperationException("Token revoked.");
        if (now >= ExpiresAtUtc) throw new InvalidOperationException("Token expired.");
        if (UsesCount >= MaxUses) throw new InvalidOperationException("Token already consumed.");

        UsesCount++;
        ConsumedAtUtc ??= now;
        ConsumedByDeviceId = deviceId;
    }

    public void Revoke(DateTime now) => RevokedAt = now;
}