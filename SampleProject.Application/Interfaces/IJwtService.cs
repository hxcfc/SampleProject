using SampleProject.Domain.Responses;
using SampleProject.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace SampleProject.Application.Interfaces
{
    /// <summary>
    /// Interface for JWT token operations (framework-agnostic)
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username</param>
        /// <param name="email">User email</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="Role">User Role as enum flags</param>
        /// <returns>Token response</returns>
        Task<TokenResponse> GenerateTokenAsync(string userId, string username, string email, string firstName, string lastName, UserRole Role);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Validates a JWT token asynchronously
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Gets user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID or null if invalid</returns>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Gets username from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Username or null if invalid</returns>
        string? GetUsernameFromToken(string token);

        /// <summary>
        /// Gets Role from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Role name or null if invalid</returns>
        string? GetRoleFromToken(string token);

        /// <summary>
        /// Gets email from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Email or null if invalid</returns>
        string? GetEmailFromToken(string token);

        /// <summary>
        /// Gets first name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>First name or null if invalid</returns>
        string? GetFirstNameFromToken(string token);

        /// <summary>
        /// Gets last name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Last name or null if invalid</returns>
        string? GetLastNameFromToken(string token);

        /// <summary>
        /// Gets full name from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Full name or null if invalid</returns>
        string? GetFullNameFromToken(string token);

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
