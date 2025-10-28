using FluentValidation;

namespace SampleProject.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Validator for ChangePasswordCommand
    /// </summary>
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters long")
                .MaximumLength(100)
                .WithMessage("New password cannot exceed 100 characters")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from current password");
        }
    }
}
