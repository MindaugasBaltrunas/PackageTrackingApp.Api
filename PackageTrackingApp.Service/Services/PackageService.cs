
using AutoMapper;
using FluentValidation;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Domain.Exceptions;
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
        private readonly IBaseRepository<Sender> _senderRepository;
        private readonly IBaseRepository<Recipient> _recipientRepository;
        private readonly IValidStatusTransition _validStatusTransitionValidator;

        public PackageService(IPackageRepository packageRepository,
            IValidator<PackageRequest> packegeValidator,
            IMapper mapper,
            IResultFactory resultFactory,
            IBaseRepository<Sender> senderRepository,
            IBaseRepository<Recipient> recipientRepository,
            IValidStatusTransition validStatusTransitionValidator
            )
        {
            _packageRepository = packageRepository;
            _packageValidator = packegeValidator;
            _mapper = mapper;
            _resultFactory = resultFactory;
            _senderRepository = senderRepository;
            _recipientRepository = recipientRepository;
            _validStatusTransitionValidator = validStatusTransitionValidator;
        }

        public async Task<Result<PackageResponse>> AddPackageAsync(PackageRequest package)
        {
            var validationResult = await _packageValidator.ValidateAsync(package);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return _resultFactory.CreateFailure<PackageResponse>(errors);
            }

            var sender = _senderRepository.GetByIdAsync(package.SenderId) ?? throw new EntityNotFoundException("sender dose not exist");
            var recipient = _recipientRepository.GetByIdAsync(package.RecipientId) ?? throw new EntityNotFoundException("recipient dose not exist");

            try
            {
                var packageEntity = _mapper.Map<Package>(package);
                packageEntity.CreatedAt = DateTime.UtcNow;
                packageEntity.CurrentStatus = 0;

                var resposePackage = await _packageRepository.AddAsync(packageEntity);

                var result = _mapper.Map<PackageResponse>(resposePackage);

                return _resultFactory.CreateSuccess(result);
            }
            catch (Exception ex)
            {
                return _resultFactory.CreateFailure<PackageResponse>($"Error adding package: {ex.Message}");
            }
        }

        public async Task<Result<List<PackageResponse>>> GetAllPackagesAsync()
        {
            var packages = await _packageRepository.GetAllAsync();
            if (packages == null || !packages.Any())
                return new Result<List<PackageResponse>>("packages not found");

            var result = _mapper.Map<List<PackageResponse>>(packages);
            return new Result<List<PackageResponse>>(result);
        }


        public async Task<Result<PackageResponse>> GetPackageAsync(string packageId)
        {
            if (!Guid.TryParse(packageId, out var id))
                return new Result<PackageResponse>("Invalid packageId");

            var package = await _packageRepository.GetAsync(id);
            if (package == null)
                return new Result<PackageResponse>("package not found");

            var result = _mapper.Map<PackageResponse>(package);

            return new Result<PackageResponse>(result);
        }

        public async Task<Result<PackageResponse>> ExchangeStatusAsync(string packageId, int status)
        {
            if (!Guid.TryParse(packageId, out var id))
                return new Result<PackageResponse>("Invalid packageId");

            var package = await _packageRepository.GetAsync(id);
            if (package == null)
                return new Result<PackageResponse>("package not found");

            var newStatus = (PackageStatus)status;

            if (package.CurrentStatus == newStatus)
            {
                var response = _mapper.Map<PackageResponse>(package);
                return new Result<PackageResponse>(response);
            }

            _validStatusTransitionValidator.Check(package.CurrentStatus, newStatus);

            package.CurrentStatus = newStatus;

            var updatedPackage = await _packageRepository.ExchangeAsync(newStatus, package);

            var result = _mapper.Map<PackageResponse>(updatedPackage);

            return new Result<PackageResponse>(result);
        }

        public async Task<Result<List<PackageResponse>>> FilterAllPackagesAsync(string? trackingNumber, int? status)
        {
            bool hasTracking = !string.IsNullOrWhiteSpace(trackingNumber);
            bool hasStatus = status.HasValue;

            if (!(hasTracking ^ hasStatus))
            {
                return new Result<List<PackageResponse>>(
                    "Provide exactly one filter: either trackingNumber OR status (but not both).");
            }

            PackageStatus? packageStatus = status.HasValue ? (PackageStatus)status.GetValueOrDefault() : null;


            var packages = await _packageRepository.FilterAllAsync(hasTracking ? trackingNumber : null, packageStatus);

            if (packages == null || !packages.Any())
            {
                return new Result<List<PackageResponse>>("No packages found");
            }

            var packageResponses = _mapper.Map<List<PackageResponse>>(packages);
            return new Result<List<PackageResponse>>(packageResponses);
        }

    }
}
