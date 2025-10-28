using SampleProject.Domain.Responses;
using SampleProject.Domain.Enums;

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
    }
}
