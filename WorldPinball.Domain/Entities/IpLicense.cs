namespace WorldPinball.Domain.Entities;

public class IpLicense
{
    private static class ErrorMessages
    {
        public const string InvalidName = "IP License name cannot be empty.";
        public const string InvalidSlug = "IP License slug cannot be empty.";
        public const string InvalidDate = "Date cannot be default.";
    }

    private IpLicense() { }

    public Guid IpId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    public Guid? PublisherId { get; private set; }
    public Publisher? Publisher { get; private set; }

    public string? IpOwner { get; private set; }
    public int? ReleaseYear { get; private set; }
    public ICollection<Region> Regions { get; private set; } = [];
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public ICollection<Machine> Machines { get; private set; } = [];

    public IpLicense(
        string name,
        string slug,
        Guid? publisherId = null,
        string? ipOwner = null,
        int? releaseYear = null,
        string? notes = null,
        bool isActive = true,
        DateTime? createdAtUtc = null,
        Guid? ipId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException(ErrorMessages.InvalidSlug, nameof(slug));

        IpId = ipId ?? Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        PublisherId = publisherId;
        IpOwner = ipOwner?.Trim();
        ReleaseYear = releaseYear;
        Notes = notes?.Trim();
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string slug,
        string? ipOwner,
        int? releaseYear,
        string? notes,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException(ErrorMessages.InvalidSlug, nameof(slug));

        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        IpOwner = ipOwner?.Trim();
        ReleaseYear = releaseYear;
        Notes = notes?.Trim();
        UpdatedAtUtc = now;
    }

    public void SetPublisher(Guid? publisherId, DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        PublisherId = publisherId;
        UpdatedAtUtc = now;
    }

    public void Activate(DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        IsActive = true;
        UpdatedAtUtc = now;
    }

    public void Deactivate(DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        IsActive = false;
        UpdatedAtUtc = now;
    }
}
