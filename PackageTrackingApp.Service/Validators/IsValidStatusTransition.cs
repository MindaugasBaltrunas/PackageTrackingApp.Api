using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Service.Validators
{
    public class IsValidStatusTransition
    {
        public bool Check(PackageStatus currentStatus, PackageStatus newStatus)
        {
            return currentStatus switch
            {
                PackageStatus.Created => newStatus is PackageStatus.Sent or PackageStatus.Cancelled,
                PackageStatus.Sent => newStatus is PackageStatus.Accepted or PackageStatus.Returned or PackageStatus.Cancelled,
                PackageStatus.Returned => newStatus is PackageStatus.Sent or PackageStatus.Cancelled,
                PackageStatus.Accepted => false,
                PackageStatus.Cancelled => false,
                _ => false
            };
        }
    }
}
