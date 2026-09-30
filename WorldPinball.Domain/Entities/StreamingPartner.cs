namespace WorldPinball.Domain.Entities;

public class StreamingPartner
{
    private static class ErrorMessages
    {
        public const string EmptyUserId = "UserId cannot be empty.";
        public const string EmptyOrganization = "Organization name cannot be empty.";
        public const string InvalidCreatedAt = "CreatedAtUtc must be a valid date.";
    }

    private StreamingPartner() { } // Used by EF Core

    public Guid PartnerId { get; private set; }
    public Guid UserId { get; private set; }
    public string OrganizationName { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public StreamingPartner(Guid userId, string organizationName, DateTime createdAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.EmptyUserId, nameof(userId));
        if (string.IsNullOrWhiteSpace(organizationName))
            throw new ArgumentException(ErrorMessages.EmptyOrganization, nameof(organizationName));
        if (createdAtUtc == default)
            throw new ArgumentException(ErrorMessages.InvalidCreatedAt, nameof(createdAtUtc));

        PartnerId = Guid.NewGuid();
        UserId = userId;
        OrganizationName = organizationName.Trim();
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }
    
    
    
    
    
    public void Revoke(DateTime revokedAtUtc)
    {
        if (revokedAtUtc < CreatedAtUtc)
            throw new ArgumentException("RevokedAtUtc cannot be earlier than CreatedAtUtc.",
                nameof(revokedAtUtc));

        // Mark partner as inactive and record timestamp
        IsActive = false;
        RevokedAtUtc = revokedAtUtc;
    }

    public void Reactivate()
    {
        // Reactivate partner and clear revocation timestamp
        IsActive = true;
        RevokedAtUtc = null;
    }
}