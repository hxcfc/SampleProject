using SampleProject.Application.Dto;
using SampleProject.Domain.Entities;

namespace SampleProject.Application.Interfaces.SampleProject.Authorization
{
    /// <summary>
    /// Interface for user authorization operations
    /// </summary>
    public interface IAuthorization
    {
        /// <summary>
        /// Refreshes a JWT token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <returns>New JWT token if refresh token is valid, null otherwise</returns>
        Task<string?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User information if credentials are valid, null otherwise</returns>
        Task<UserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves refresh token for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="expiryTime">Token expiry time</param>
        /// <returns>True if saved successfully, false otherwise</returns>
        Task<bool> SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime);

        /// <summary>
        /// Validates refresh token and returns user information
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate</param>
        /// <returns>User information if token is valid, null otherwise</returns>
        Task<UserDto?> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes refresh token for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if revoked successfully, false otherwise</returns>
        Task<bool> RevokeRefreshTokenAsync(Guid userId);

        /// <summary>
        /// Revokes all refresh tokens for user (security measure)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if revoked successfully, false otherwise</returns>
        Task<bool> RevokeAllRefreshTokensAsync(Guid userId);

        /// <summary>
        /// Gets user entity by refresh token for security validation
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>User entity or null if not found</returns>
        Task<UserEntity?> GetUserEntityByRefreshTokenAsync(string refreshToken);
    }
}