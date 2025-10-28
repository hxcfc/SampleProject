using SampleProject.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SampleProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Extended JWT service interface with ASP.NET Core specific functionality
    /// </summary>
    public interface IExtendedJwtService : IJwtService
    {
        /// <summary>
        /// Sets JWT tokens as HTTP-only cookies
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="refreshToken">Refresh token</param>
        void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken);

        /// <summary>
        /// Gets access token from cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Access token or null if not found</returns>
        string? GetAccessTokenFromCookies(HttpRequest request);

        /// <summary>
        /// Gets refresh token from cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Refresh token or null if not found</returns>
        string? GetRefreshTokenFromCookies(HttpRequest request);

        /// <summary>
        /// Clears JWT token cookies
        /// </summary>
        /// <param name="response">HTTP response</param>
        void ClearTokenCookies(HttpResponse response);

        /// <summary>
        /// Gets token from either Authorization header or cookies
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Token or null if not found</returns>
        string? GetTokenFromRequest(HttpRequest request);
    }
}
