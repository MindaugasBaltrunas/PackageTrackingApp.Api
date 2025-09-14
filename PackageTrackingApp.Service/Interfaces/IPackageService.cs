using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.Interfaces
{
    public interface IPackageService
    {
        Task<Result<PackageResponse>> AddPackageAsync(PackageRequest package);
        Task<Result<List<PackageResponse>>> GetAllPackagesAsync();
        Task<Result<PackageResponse>> GetPackageAsync(string packageId);
        Task<Result<List<PackageResponse>>> FilterAllPackagesAsync(string? trackingNumber, int? status);
        Task<Result<PackageResponse>> ExchangeStatusAsync(string packageId, int status);
        Task<Result<List<PackageStatusHistoryResponse>>> GetStatusHistory(string packageId);
    }
}
