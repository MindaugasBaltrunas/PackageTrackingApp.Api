
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Interfaces
{
    public interface IValidStatusTransition
    {
        bool Check(PackageStatus currentStatus, PackageStatus newStatus);
    }
}
