using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class EnrollmentToken
{
    private static class ErrorMessages
    {
        public const string TokenHash = "The provided token hash is invalid / empty.";
        public const string TokenType = "Invalid token type.";
        public const string IssuedByUserId = "The provided issued by is invalid / empty.";
        public const string IssuedAt = "Issued date cannot be empty.";
    }

    public Guid TokenId { get; private set; }

    // Hash only (never store raw token)
    public string TokenHash { get; private set; }
    public EnrollmentTokenType TokenType { get; private set; } = EnrollmentTokenType.DeviceEnroll;

    // Optional scoping
    public Guid? MachineId { get; private set; }
    public Guid? DeviceId { get; private set; }

    public string IssuedByUserId { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }
    public Guid? ConsumedByDeviceId { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public int MaxUses { get; private set; }
    public int UsesCount { get; private set; } 

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
        string? notes = null
        )
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentNullException( nameof(tokenHash),ErrorMessages.TokenHash);
        
        if (tokenType is not EnrollmentTokenType.DeviceEnroll)
            throw new ArgumentNullException( nameof(tokenType),ErrorMessages.TokenType);
        
        if (issuedByUserId == string.Empty)
            throw new ArgumentNullException(nameof(issuedByUserId), ErrorMessages.IssuedByUserId);
        
        if (issuedAt ==  default)
            throw new ArgumentNullException( nameof(issuedAt),ErrorMessages.IssuedAt);
    
        TokenId = Guid.NewGuid();
        TokenHash = tokenHash;
        TokenType = tokenType;
        IssuedByUserId = issuedByUserId;
        IssuedAtUtc = issuedAt;
        ExpiresAtUtc = expiresAt;
        MachineId = machineId;
        DeviceId = deviceId;
        MaxUses = maxUses <= 0 ? 1 : maxUses;
        Notes = notes;
        UsesCount = 0;
    }

    public void Consume(Guid deviceId, DateTime now)
    {
        if (RevokedAt is not null) throw new InvalidOperationException("Token revoked.");
        if (now >= ExpiresAtUtc) throw new InvalidOperationException("Token expired."); ;
        if (UsesCount >= MaxUses) throw new InvalidOperationException("Token already consumed.");
        if ( DeviceId.HasValue && deviceId != DeviceId) throw new InvalidOperationException("Token has different device Id");
     
        
        UsesCount++;
        ConsumedAtUtc ??= now;
        ConsumedByDeviceId = deviceId;
    }

    public void Revoke(DateTime now) => RevokedAt = now;
}