
using AutoMapper;
using FluentValidation;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Domain.Interfaces;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Service.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IValidator<PackageRequest> _packageValidator;
        private readonly IMapper _mapper;
        private readonly IResultFactory _resultFactory;
        public PackageService(IPackageRepository packageRepository,
            IValidator<PackageRequest> packegeValidator,
            IMapper mapper,
            IResultFactory resultFactory)
        {
            _packageRepository = packageRepository;
            _packageValidator = packegeValidator;
            _mapper = mapper;
            _resultFactory = resultFactory;
        }

        public async Task<Result<Package>> AddPackageAsync(PackageRequest package)
        {
            var validationResult = await _packageValidator.ValidateAsync(package);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return _resultFactory.CreateFailure<Package>(errors);
            }

            try
            {
                var packageEntity = _mapper.Map<Package>(package);
                packageEntity.CreatedAt = DateTime.UtcNow;

                var resposePackage = await _packageRepository.AddAsync(packageEntity);

                return _resultFactory.CreateSuccess(resposePackage);
            }
            catch (Exception ex)
            {
                return _resultFactory.CreateFailure<Package>($"Error adding package: {ex.Message}");
            }
        }

        public async Task<List<Package>> GetAllPackagesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Package>> FilterAllPackagesAsync(int? trackingNumber, string? status)
        {
            throw new NotImplementedException();
        }

        public Task<Package> ExchangeStatusAsync(string packageId, string status, string prevStatus)
        {

            throw new NotImplementedException();
        }
    }
}
