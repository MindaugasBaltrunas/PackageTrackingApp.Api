using FluentValidation;
using PackageTrackingApp.Domain.Interfaces;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Service.Services
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IBaseRepository<T> _baseRepository;
        private readonly IValidator<T> _validator;
        private readonly IResultFactory _resultFactory;

        public BaseService(
            IBaseRepository<T> baseRepository,
            IValidator<T> validator,
            IResultFactory resultFactory)
        {
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        }

        public async Task<Result<T>> AddEntityAsync(T entity)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(entity);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return _resultFactory.CreateFailure<T>(errors);
                }

                var addedEntity = await _baseRepository.AddAsync(entity);
                return _resultFactory.CreateSuccess(addedEntity);
            }
            catch (Exception ex)
            {
                return _resultFactory.CreateFailure<T>(new List<string> { ex.Message });
            }
        }
    }
}
