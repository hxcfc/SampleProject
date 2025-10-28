using FluentValidation;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Validator for ChangePasswordCommand
    /// </summary>
    public class ChangePasswordCommandValidator : BaseValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .Must(BeValidGuid)
                .WithMessage("User ID cannot be empty GUID");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required")
                .Must(NotContainDangerousCharacters)
                .WithMessage("Current password contains invalid characters");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters long")
                .MaximumLength(100)
                .WithMessage("New password cannot exceed 100 characters")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from current password")
                .Must(NotContainDangerousCharacters)
                .WithMessage("New password contains invalid characters");
        }
    }
}
