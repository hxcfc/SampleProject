using MediatR;
using SampleProject.Domain.Common;

namespace SampleProject.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Command to change user password
    /// </summary>
    public class ChangePasswordCommand : IRequest<Result<bool>>
    {
        /// <summary>
        /// User ID to change password for
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Current password for verification
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of ChangePasswordCommand
        /// </summary>
        /// <param name="userId">User ID to change password for</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        public ChangePasswordCommand(Guid userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }
}
