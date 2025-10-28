using FluentValidation;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Validator for LoginCommand
    /// </summary>
    public class LoginCommandValidator : BaseValidator<LoginCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class
        /// </summary>
        public LoginCommandValidator()
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