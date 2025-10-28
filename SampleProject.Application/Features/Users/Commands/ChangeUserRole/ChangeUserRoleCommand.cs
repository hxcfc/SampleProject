using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Features.Users.Commands.ChangeUserRole
{
    /// <summary>
    /// Command to change user role (Admin only)
    /// </summary>
    public class ChangeUserRoleCommand : IRequest<Result<UserDto>>
    {
        /// <summary>
        /// User ID to change role for
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// New user role
        /// </summary>
        public UserRole NewRole { get; set; }

        /// <summary>
        /// Initializes a new instance of ChangeUserRoleCommand
        /// </summary>
        /// <param name="userId">User ID to change role for</param>
        /// <param name="newRole">New user role</param>
        public ChangeUserRoleCommand(Guid userId, UserRole newRole)
        {
            UserId = userId;
            NewRole = newRole;
        }
    }
}
