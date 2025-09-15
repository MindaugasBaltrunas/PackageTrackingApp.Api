
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Dtos
{
    public class PackageStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public required PackageStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
