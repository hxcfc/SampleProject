using FluentValidation;

namespace SampleProject.Application.Features.Users.Queries.CheckEmailAvailability
{
    /// <summary>
    /// Validator for CheckEmailAvailabilityQuery
    /// </summary>
    public class CheckEmailAvailabilityQueryValidator : AbstractValidator<CheckEmailAvailabilityQuery>
    {
        public CheckEmailAvailabilityQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");
        }
    }
}
