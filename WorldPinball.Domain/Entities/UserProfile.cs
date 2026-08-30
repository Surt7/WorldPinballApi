

namespace WorldPinball.Domain.Entities
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = "No Name";
        public string AvatarUrl { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool IsProfilePublic { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; } = null;

    }
}
