using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;

namespace SampleProject.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Command to refresh a JWT token
    /// </summary>
    public record RefreshTokenCommand : IRequest<Result<TokenResponse>>
    {
        /// <summary>
        /// The refresh token
        /// </summary>
        public required string RefreshToken { get; init; }
    }
}