

namespace PackageTrackingApp.Domain.Entities
{
    public class Package
    {
        public Guid Id { get; set; }

        public required string TrackingNumber { get; set; }
        public required PackageStatus CurrentStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid SenderId { get; set; }
        public required Sender Sender { get; set; }

        public Guid RecipientId { get; set; }
        public required Sender Recipient { get; set; }

        public required ICollection<PackageStatusHistory> StatusHistory { get; set; }
    }
}
