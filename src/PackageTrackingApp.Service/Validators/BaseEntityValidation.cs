using FluentValidation;
using PackageTrackingApp.Domain.Entities;

public abstract class BaseEntityValidator<T> : AbstractValidator<T> where T : BaseEntity
{
    protected BaseEntityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Name can only contain letters, spaces, hyphens, apostrophes, and periods");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .Length(5, 250).WithMessage("Address must be between 5 and 250 characters")
            .Matches(@"^[a-zA-Z0-9\s,\-#\.\/]+$").WithMessage("Address contains invalid characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Length(10, 15).WithMessage("Phone number must be between 10 and 15 digits")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Phone number format is invalid");
    }
}
