using FluentValidation;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Validator for UpdateUserCommand
    /// </summary>
    public class UpdateUserCommandValidator : BaseValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .Must(BeValidGuid)
                .WithMessage("User ID cannot be empty GUID");

            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("First name contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("Last name contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters")
                .Must(BeValidEmailFormat)
                .WithMessage("Email contains invalid characters")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
