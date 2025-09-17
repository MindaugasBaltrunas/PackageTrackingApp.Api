
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
        private readonly IBaseRepository<Sender> _senderRepository;
        private readonly IBaseRepository<Recipient> _recipientRepository;
        private readonly IBaseRepository<PackageStatusHistory> _packegeStausrRepository;
        private readonly IValidStatusTransition _validStatusTransitionValidator;

        public PackageService(IPackageRepository packageRepository,
            IValidator<PackageRequest> packageValidator,
            IMapper mapper,
            IResultFactory resultFactory,
            IBaseRepository<Sender> senderRepository,
            IBaseRepository<Recipient> recipientRepository,
            IBaseRepository<PackageStatusHistory> packegeStausrRepository,
            IValidStatusTransition validStatusTransitionValidator
            )
        {
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
            _senderRepository = senderRepository ?? throw new ArgumentNullException(nameof(senderRepository));
            _recipientRepository = recipientRepository ?? throw new ArgumentNullException(nameof(recipientRepository));
            _packegeStausrRepository = packegeStausrRepository ?? throw new ArgumentNullException(nameof(packegeStausrRepository));
            _validStatusTransitionValidator = validStatusTransitionValidator ?? throw new ArgumentNullException(nameof(validStatusTransitionValidator));
        }

        public async Task<Result<PackageResponse>> AddPackageAsync(PackageRequest package)
        {
            var validationResult = await _packageValidator.ValidateAsync(package);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return _resultFactory.CreateFailure<PackageResponse>(errors);
            }

            var sender = await _senderRepository.GetByIdAsync(package.SenderId);
            if (sender == null)
                return _resultFactory.CreateFailure<PackageResponse>("Sender does not exist");

            var recipient = await _recipientRepository.GetByIdAsync(package.RecipientId);
            if (recipient == null)
                return _resultFactory.CreateFailure<PackageResponse>("Recipient does not exist");

            try
            {
                var packageEntity = _mapper.Map<Package>(package);
                packageEntity.CreatedAt = DateTime.UtcNow;
                packageEntity.CurrentStatus = PackageStatus.Created;

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
            if (!packages.Any())
                return new Result<List<PackageResponse>>("packages not found");

            var result = _mapper.Map<List<PackageResponse>>(packages);
            return new Result<List<PackageResponse>>(result);
        }


        public async Task<Result<PackageResponse>> GetPackageByIdAsync(string packageId)
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

            if (!_validStatusTransitionValidator.Check(package.CurrentStatus, newStatus))
                return new Result<PackageResponse>("Invalid status transition");

            var statusHistory = new PackageStatusHistory
            {
                Id = Guid.NewGuid(),
                PackageId = package.Id,
                Status = package.CurrentStatus,
                ChangedAt = DateTime.UtcNow
            };

            await _packegeStausrRepository.AddAsync(statusHistory);

            var updatedPackage = await _packageRepository.UpdateAsync(newStatus, package);

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

            if (!packages.Any())
            {
                return new Result<List<PackageResponse>>("No packages found");
            }

            var packageResponses = _mapper.Map<List<PackageResponse>>(packages);
            return new Result<List<PackageResponse>>(packageResponses);
        }

        public async Task<Result<List<PackageStatusHistoryResponse>>> GetStatusHistory(string packageId)
        {
            if (!Guid.TryParse(packageId, out var id))
                return new Result<List<PackageStatusHistoryResponse>>("Invalid packageId");

            var package = await _packageRepository.GetAsync(id);
            if (package == null)
                return new Result<List<PackageStatusHistoryResponse>>("package not found");

            var result = _mapper.Map<List<PackageStatusHistoryResponse>>(package.StatusHistory);

            return new Result<List<PackageStatusHistoryResponse>>(result);
        }
    }
}
