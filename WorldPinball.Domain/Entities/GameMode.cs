namespace WorldPinball.Domain.Entities;

public class GameMode
{
    public Guid ModeId { get; set; }
    public string Code { get; set; } = null!; // survival, normal, story...
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
