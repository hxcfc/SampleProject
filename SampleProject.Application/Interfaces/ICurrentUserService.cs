using SampleProject.Domain.Responses;

namespace SampleProject.Application.Interfaces
{
    /// <summary>
    /// Service for accessing current authenticated user information
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current authenticated user information
        /// </summary>
        /// <returns>Current user DTO or null if not authenticated</returns>
        CurrentUserResponse? GetCurrentUser();

        /// <summary>
        /// Gets the current user email
        /// </summary>
        /// <returns>User email or null if not authenticated</returns>
        string? GetCurrentUserEmail();

        /// <summary>
        /// Gets the current user ID
        /// </summary>
        /// <returns>User ID or null if not authenticated</returns>
        string? GetCurrentUserId();

        /// <summary>
        /// Gets the current user Role
        /// </summary>
        /// <returns>List of user Role or empty list if not authenticated</returns>
        string? GetCurrentUserRole();

        /// <summary>
        /// Checks if the current user has specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        bool HasRole(string role);

        /// <summary>
        /// Checks if the current user has admin role
        /// </summary>
        /// <returns>True if user is admin, false otherwise</returns>
        bool IsAdmin();

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if user is authenticated, false otherwise</returns>
        bool IsAuthenticated();
    }
}