using System.Text.Json.Serialization;

namespace WorldPinball.Domain.Entities;

public sealed class Publisher
{
    public Guid PublisherId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    [JsonIgnore]
    public ICollection<IpLicense> IpLicenses { get; set; } = [];
}
