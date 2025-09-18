
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Dtos
{
    public class PackageResponse
    {
        public  Guid Id { get; set; }
        public required string TrackingNumber { get; set; }
        public required PackageStatus CurrentStatus { get; set; }

        public required SenderDto Sender { get; set; }

        public required RecipientDto Recipient { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
