namespace WorldPinball.Domain.Entities;

public class UserProfile
{
    private static readonly HashSet<string> ReservedDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Administrator",
        "System",
        "WorldPinball",
        "No Name",
        "Anonymous"
    };

    private static class ErrorMessages
    {
        public const string InvalidUserId = "UserId cannot be empty.";
        public const string InvalidDisplayName = "DisplayName cannot be empty.";
        public const string ReservedDisplayName = "DisplayName is reserved and cannot be used.";
        public const string InvalidDate = "Date cannot be default.";
    }

    private UserProfile() { }

    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = "No Name";
    public string AvatarUrl { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public bool IsProfilePublic { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public UserProfile(
        Guid userId,
        string displayName,
        string avatarUrl = "",
        string countryCode = "",
        bool isProfilePublic = false,
        DateTime? createdAtUtc = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.InvalidUserId, nameof(userId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException(ErrorMessages.InvalidDisplayName, nameof(displayName));

        var trimmedName = displayName.Trim();
        if (ReservedDisplayNames.Contains(trimmedName))
            throw new ArgumentException(ErrorMessages.ReservedDisplayName, nameof(displayName));

        UserId = userId;
        DisplayName = trimmedName;
        AvatarUrl = avatarUrl?.Trim() ?? string.Empty;
        CountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;
        IsProfilePublic = isProfilePublic;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void UpdateProfile(
        string displayName,
        string avatarUrl,
        string countryCode,
        bool isProfilePublic,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException(ErrorMessages.InvalidDisplayName, nameof(displayName));

        var trimmedName = displayName.Trim();
        if (ReservedDisplayNames.Contains(trimmedName))
            throw new ArgumentException(ErrorMessages.ReservedDisplayName, nameof(displayName));

        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        DisplayName = trimmedName;
        AvatarUrl = avatarUrl?.Trim() ?? string.Empty;
        CountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;
        IsProfilePublic = isProfilePublic;
        UpdatedAtUtc = now;
    }
}
