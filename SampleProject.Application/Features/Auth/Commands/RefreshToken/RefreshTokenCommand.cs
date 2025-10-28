using MediatR;
using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;

namespace SampleProject.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    /// Command to refresh a JWT token using refresh token from HTTP-only cookies
    /// </summary>
    public record RefreshTokenCommand : IRequest<Result<TokenResponse>>
    {
        // No properties needed - refresh token will be extracted from cookies
        // This enables stateless refresh via cookie authentication
    }
}