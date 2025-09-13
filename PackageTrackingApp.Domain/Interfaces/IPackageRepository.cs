
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Domain.Interfaces
{
    public interface IPackageRepository
    {
        Task<Package> AddAsync(Package package);
        Task<List<Package>> GetAllAsync();
        Task<List<Package>> FilterAllAsync(int? trackingNumber, string? status);
        Task<Package?> ExchangeAsync(PackageStatus status, Guid id);
    }
}
