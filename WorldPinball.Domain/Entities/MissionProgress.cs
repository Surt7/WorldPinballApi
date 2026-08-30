
using WorldPinball.Domain.Enums;
namespace WorldPinball.Domain.Entities
{
    public class MissionProgress
    {
        public Guid MissionProgressId { get; set; }
        public Guid MissionId { get; set; }
        public Mission Mission { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public int CurrentValue { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public MissionProgressStatus Status { get; set; } = MissionProgressStatus.NotStarted; // in_progress | completed | failed
    }
}
