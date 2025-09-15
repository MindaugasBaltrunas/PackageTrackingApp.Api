

namespace PackageTrackingApp.Service.Dtos
{
    public class PackageRequest
    {
        public required string TrackingNumber { get; set; }
        public required Guid SenderId { get; set; }
        public required Guid RecipientId { get; set; }
    }
}
