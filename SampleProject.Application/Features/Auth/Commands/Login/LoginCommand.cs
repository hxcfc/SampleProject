using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;
using MediatR;

namespace SampleProject.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Command to authenticate a user
    /// </summary>
    public record LoginCommand : IRequest<Result<TokenResponse>>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// User's password
        /// </summary>
        public required string Password { get; init; }
    }
}