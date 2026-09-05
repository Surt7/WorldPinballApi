namespace WorldPinball.Domain.Entities
{
    public class Mission
    {
        public required Guid MissionId { get; set; }
        public Guid? IpId { get; set; }
        public IpLicense? IpLicense { get; set; }
        public Guid? ModeId { get; set; }
        public GameMode? GameMode { get; set; }
        public string Code { get; set; } = null!; // unique code for the mission
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string MissionType { get; set; } = null!; // e.g., "score_target", "time_limit", "multiball", etc.
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        // ✅ relation pivot
        

    }
}
