using FluentValidation;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Validator for CreateUserCommand
    /// </summary>
    public class CreateUserCommandValidator : BaseValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters")
                .Must(BeValidEmailFormat)
                .WithMessage("Email contains invalid characters");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("First name contains invalid characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("Last name contains invalid characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters")
                .Must(NotContainDangerousCharacters)
                .WithMessage("Password contains invalid characters");
        }

    }
}