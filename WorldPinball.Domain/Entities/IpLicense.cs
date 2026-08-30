

namespace WorldPinball.Domain.Entities;

public class IpLicense
{
    public Guid IpId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? PublisherId { get; set; }      // FK
    public Publisher? Publisher { get; set; }   // navigation

    public string? IpOwner { get; set; }
    public int? ReleaseYear { get; set; }
    public ICollection<Region> Regions { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation
    public ICollection<Machine> Machines { get; set; } = [];
}
