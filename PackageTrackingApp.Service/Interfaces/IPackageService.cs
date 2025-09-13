using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.Interfaces
{
    public interface IPackageService
    {
        Task<Result<PackageResponse>> AddPackageAsync(PackageRequest package);
        Task<List<Package>> GetAllPackagesAsync();
        Task<List<Package>> FilterAllPackagesAsync(int? trackingNumber, string? status);
        Task<Package> ExchangeStatusAsync(string packageId, string status, string prevStatus);
    }
}
