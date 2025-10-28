using SampleProject.Domain.Common;
using SampleProject.Domain.Dto;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Interfaces
{
    /// <summary>
    /// Service for user management operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user (registration)
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <param name="password">User's password</param>
        /// <returns>Result containing created user DTO or error</returns>
        Task<Result<UserDto>> CreateUserAsync(string email, string firstName, string lastName, string password);

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="userId">User's unique identifier</param>
        /// <returns>Result containing user DTO or error</returns>
        Task<Result<UserDto>> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>Result containing user entity or error</returns>
        Task<Result<UserEntity>> GetUserByEmailAsync(string email);

        /// <summary>
        /// Checks if a user with the given email already exists
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> UserExistsAsync(string email);

        /// <summary>
        /// Gets a paginated list of users with filters
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="filters">Search and filter criteria</param>
        /// <returns>Result containing paged user list or error</returns>
        Task<Result<PagedResult<UserDto>>> GetUsersListAsync(int page, int pageSize, UserFilters filters);

        /// <summary>
        /// Updates user information
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="firstName">New first name (optional)</param>
        /// <param name="lastName">New last name (optional)</param>
        /// <param name="email">New email (optional)</param>
        /// <param name="isActive">New active status (optional, Admin only)</param>
        /// <param name="isEmailVerified">New email verification status (optional, Admin only)</param>
        /// <param name="role">New role (optional, Admin only)</param>
        /// <returns>Result containing updated user DTO or error</returns>
        Task<Result<UserDto>> UpdateUserAsync(Guid userId, string? firstName, string? lastName, string? email, bool? isActive, bool? isEmailVerified, UserRole? role);

        /// <summary>
        /// Changes user password
        /// </summary>
        /// <param name="userId">User ID to change password for</param>
        /// <param name="currentPassword">Current password for verification</param>
        /// <param name="newPassword">New password</param>
        /// <returns>Result indicating success or failure</returns>
        Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// Changes user role (Admin only)
        /// </summary>
        /// <param name="userId">User ID to change role for</param>
        /// <param name="newRole">New user role</param>
        /// <returns>Result containing updated user DTO or error</returns>
        Task<Result<UserDto>> ChangeUserRoleAsync(Guid userId, UserRole newRole);
    }
}
