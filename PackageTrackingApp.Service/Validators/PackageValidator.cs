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
                .Length(1, 50); 

            RuleFor(x => x.CurrentStatus)
                .IsInEnum().WithMessage("Invalid package status");

            RuleFor(x => x.SenderId)
                .GreaterThan(0).WithMessage("SenderId must be greater than 0");

            RuleFor(x => x.RecipientId)
                .GreaterThan(0).WithMessage("RecipientId must be greater than 0");
        }
    }
}
