
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Dtos
{
    public class PackageResponse
    {
        public required string TrackingNumber { get; set; }
        public required PackageStatus CurrentStatus { get; set; }
    }
}
