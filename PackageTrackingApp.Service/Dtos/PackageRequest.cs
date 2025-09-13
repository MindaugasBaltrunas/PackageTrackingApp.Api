
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Dtos
{
    public class PackageRequest
    {
        public required string TrackingNumber { get; set; }
        public required PackageStatus CurrentStatus { get; set; }
        public required int SenderId { get; set; }
        public required int RecipientId { get; set; }
    }
}
