namespace WorldPinball.Domain.Entities;

public sealed class Region
{
    private static class ErrorMessages
    {
        public const string InvalidCode = "Region code cannot be empty.";
        public const string InvalidName = "Region name cannot be empty.";
        public const string InvalidDate = "Date cannot be default.";
    }

    private Region() { }

    public Guid RegionId { get; private set; }
    public string Code { get; private set; } = null!; // e.g. "US", "EU", "MENA"
    public string Name { get; private set; } = null!; // e.g. "United States", "Europe"

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public ICollection<IpLicense> IpLicenses { get; private set; } = [];

    public Region(string code, string name, DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(ErrorMessages.InvalidCode, nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        RegionId = Guid.NewGuid();
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void Update(string name, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        Name = name.Trim();
        UpdatedAtUtc = now;
    }
}
