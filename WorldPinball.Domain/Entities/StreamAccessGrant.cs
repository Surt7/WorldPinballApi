using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class StreamAccessGrant
{
    private StreamAccessGrant() { } // Requis par EF Core

    public Guid GrantId { get; private set; }
    public PrincipalType PrincipalType { get; private set; }
    public Guid PrincipalId { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public GrantOrigin Origin { get; private set; }
    public GrantStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }
    public Guid? GrantedByUserId { get; private set; }

    public StreamAccessGrant(
        PrincipalType principalType,
        Guid principalId,
        string displayName,
        GrantOrigin origin,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc = null,
        Guid? grantedByUserId = null)
    {
        if (principalId == Guid.Empty)
            throw new ArgumentException("PrincipalId cannot be empty.", nameof(principalId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(displayName));

        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= createdAtUtc)
            throw new ArgumentException("ExpiresAtUtc must be in the future.", nameof(expiresAtUtc));

        GrantId = Guid.NewGuid();
        PrincipalType = principalType;
        PrincipalId = principalId;
        DisplayName = displayName.Trim();
        Origin = origin;
        Status = GrantStatus.Active;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        GrantedByUserId = grantedByUserId;
    }

    public void Revoke(string reason, DateTime revokedAtUtc)
    {
        if (Status is GrantStatus.Revoked)
            throw new InvalidOperationException("Grant is already revoked.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A revocation reason is required.", nameof(reason));

        Status = GrantStatus.Revoked;
        RevokedAtUtc = revokedAtUtc;
        RevocationReason = reason.Trim();
    }

    public void Reactivate()
    {
        Status = GrantStatus.Active;
        RevokedAtUtc = null;
        RevocationReason = null;
    }

    public bool IsActive(DateTime nowUtc)
    {
        if (Status != GrantStatus.Active)
            return false;

        if (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= nowUtc)
            return false;

        return true;
    }
}