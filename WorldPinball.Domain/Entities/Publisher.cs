using System.Text.Json.Serialization;

namespace WorldPinball.Domain.Entities;

public sealed class Publisher
{
    private static class ErrorMessages
    {
        public const string InvalidName = "Publisher name cannot be empty.";
        public const string InvalidSlug = "Publisher slug cannot be empty.";
        public const string InvalidDate = "Date cannot be default.";
    }

    private Publisher() { }

    public Guid PublisherId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    [JsonIgnore]
    public ICollection<IpLicense> IpLicenses { get; private set; } = [];

    public Publisher(string name, string slug, DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException(ErrorMessages.InvalidSlug, nameof(slug));

        PublisherId = Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void Update(string name, string slug, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException(ErrorMessages.InvalidSlug, nameof(slug));

        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        UpdatedAtUtc = now;
    }
}
