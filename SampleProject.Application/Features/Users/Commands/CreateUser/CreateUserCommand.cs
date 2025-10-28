using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;

namespace SampleProject.Application.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Command to create a new user (registration)
    /// </summary>
    public record CreateUserCommand : IRequest<Result<UserDto>>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// User's first name
        /// </summary>
        public required string FirstName { get; init; }

        /// <summary>
        /// User's last name
        /// </summary>
        public required string LastName { get; init; }

        /// <summary>
        /// User's password
        /// </summary>
        public required string Password { get; init; }
    }
}
