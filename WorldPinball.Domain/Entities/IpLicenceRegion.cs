

namespace WorldPinball.Domain.Entities
{
    public class IpLicenceRegion
    {
        public Guid IpId { get; set; }
        public IpLicense? IpLicenses { get; set; }

        public Guid RegionId { get; set; }
        public Region? Regions { get; set; }
    }
}
