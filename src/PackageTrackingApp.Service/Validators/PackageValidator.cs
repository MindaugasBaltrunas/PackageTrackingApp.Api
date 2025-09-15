using FluentValidation;
using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.Validators
{
    public class PackageValidator : AbstractValidator<PackageRequest>
    {
        public PackageValidator()
        {
            RuleFor(x => x.TrackingNumber)
                .NotEmpty().WithMessage("Tracking number is required")
                .Length(1, 50).WithMessage("Tracking number must be between 1 and 50 characters");

            RuleFor(x => x.SenderId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("SenderId is required")
                .NotEqual(Guid.Empty).WithMessage("SenderId cannot be an empty GUID");

            RuleFor(x => x.RecipientId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("RecipientId is required")
                .NotEqual(Guid.Empty).WithMessage("RecipientId cannot be an empty GUID");
        }
    }
}
