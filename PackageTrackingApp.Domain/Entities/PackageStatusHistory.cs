
namespace PackageTrackingApp.Domain.Entities
{
    public class PackageStatusHistory
    {
        public Guid Id { get; set; }           
        public Guid PackageId { get; set; }   
        public Package? Package { get; set; }

        public required PackageStatus Status { get; set; }  
        public DateTime ChangedAt { get; set; }
    }
}
