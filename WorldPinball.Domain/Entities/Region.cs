namespace WorldPinball.Domain.Entities;

public sealed class Region
{
    public Guid RegionId { get; set; }
    public string Code { get; set; } = null!; // ex: "US", "EU", "MENA"
    public string Name { get; set; } = null!; // ex: "United States", "Europe"

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<IpLicense> IpLicenses { get; set; } = [];
}
