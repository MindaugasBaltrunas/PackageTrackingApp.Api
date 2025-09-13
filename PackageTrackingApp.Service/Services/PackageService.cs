
using AutoMapper;
using FluentValidation;
using PackageTrackingApp.Data.Repositories;
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

        public PackageService(IPackageRepository packageRepository,
            IValidator<PackageRequest> packegeValidator,
            IMapper mapper,
            IResultFactory resultFactory,
            IBaseRepository<Sender> senderRepository,
            IBaseRepository<Recipient> recipientRepository
            )
        {
            _packageRepository = packageRepository;
            _packageValidator = packegeValidator;
            _mapper = mapper;
            _resultFactory = resultFactory;
            _senderRepository = senderRepository;
            _recipientRepository = recipientRepository;
        }

        public async Task<Result<PackageResponse>> AddPackageAsync(PackageRequest package)
        {
            var validationResult = await _packageValidator.ValidateAsync(package);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return _resultFactory.CreateFailure<PackageResponse>(errors);
            }

            var sender = _senderRepository.GetByIdAsync(package.SenderId);
            if (sender == null)
                throw new EntityNotFoundException("sender dose not exist");

            var recipient = _recipientRepository.GetByIdAsync(package.RecipientId);
            if (recipient == null)
                throw new EntityNotFoundException("recipient dose not exist");

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
