using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Application.Dto;

namespace SampleProject.Application.Features.Users.Queries.GetUserById
{
    /// <summary>
    /// Query to get a user by their ID
    /// </summary>
    public record GetUserByIdQuery : IRequest<Result<UserDto>>
    {
        /// <summary>
        /// The unique identifier of the user
        /// </summary>
        public required Guid UserId { get; init; }
    }
}