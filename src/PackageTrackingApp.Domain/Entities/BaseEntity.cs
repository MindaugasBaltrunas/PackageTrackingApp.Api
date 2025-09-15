
namespace PackageTrackingApp.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string Phone { get; set; }
    }
}
