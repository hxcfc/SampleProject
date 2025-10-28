using FluentValidation;
using SampleProject.Domain.Enums;
using SampleProject.Application.Common;

namespace SampleProject.Application.Features.Users.Commands.ChangeUserRole
{
    /// <summary>
    /// Validator for ChangeUserRoleCommand
    /// </summary>
    public class ChangeUserRoleCommandValidator : BaseValidator<ChangeUserRoleCommand>
    {
        public ChangeUserRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .Must(BeValidGuid)
                .WithMessage("User ID cannot be empty GUID");

            RuleFor(x => x.NewRole)
                .Must(BeValidSingleRole)
                .WithMessage("Role must be either User or Admin (single role only)")
                .NotEqual(UserRole.None)
                .WithMessage("Role cannot be None");
        }

        /// <summary>
        /// Validates that the role is a single valid role (not a combination)
        /// </summary>
        /// <param name="role">Role to validate</param>
        /// <returns>True if valid single role</returns>
        private static bool BeValidSingleRole(UserRole role)
        {
            return role == UserRole.User || role == UserRole.Admin;
        }
    }
}
