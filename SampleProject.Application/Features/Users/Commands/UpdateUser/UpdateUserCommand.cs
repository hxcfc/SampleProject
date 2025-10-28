using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Domain.Dto;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Command to update user information (without password)
    /// </summary>
    public class UpdateUserCommand : IRequest<Result<UserDto>>
    {
        /// <summary>
        /// User ID to update
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// User's active status (Admin only)
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// User's email verification status (Admin only)
        /// </summary>
        public bool? IsEmailVerified { get; set; }

        /// <summary>
        /// Initializes a new instance of UpdateUserCommand
        /// </summary>
        /// <param name="userId">User ID to update</param>
        public UpdateUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }
}
