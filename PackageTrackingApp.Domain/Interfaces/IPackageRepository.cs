
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Domain.Interfaces
{
    public interface IPackageRepository
    {
        Task<Package> AddAsync(Package package);
        Task<List<Package>> GetAllAsync();
        Task<Package?> GetAsync(Guid id);
        Task<List<Package>> FilterAllAsync(string? trackingNumber, PackageStatus? status);
        Task<Package?> ExchangeAsync(PackageStatus status, Package package);
    }
}
