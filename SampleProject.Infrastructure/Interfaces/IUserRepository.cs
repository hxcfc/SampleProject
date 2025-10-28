using SampleProject.Domain.Entities;
using SampleProject.Domain.Dto;

namespace SampleProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Repository interface for user data access
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">User entity to create</param>
        /// <returns>Created user entity or null if failed</returns>
        Task<UserEntity?> CreateAsync(UserEntity user);

        /// <summary>
        /// Gets a user by their unique identifier
        /// </summary>
        /// <param name="id">User's unique identifier</param>
        /// <returns>User entity or null if not found</returns>
        Task<UserEntity?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>User entity or null if not found</returns>
        Task<UserEntity?> GetByEmailAsync(string email);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User entity to update</param>
        /// <returns>Updated user entity or null if failed</returns>
        Task<UserEntity?> UpdateAsync(UserEntity user);

        /// <summary>
        /// Deletes a user by their unique identifier
        /// </summary>
        /// <param name="id">User's unique identifier</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets a paginated list of users with filters
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="filters">Search and filter criteria</param>
        /// <returns>Paged result with user entities</returns>
        Task<PagedResult<UserEntity>> GetUsersListAsync(int page, int pageSize, UserFilters filters);

        /// <summary>
        /// Updates user information
        /// </summary>
        /// <param name="user">User entity with updated information</param>
        /// <returns>Updated user entity or null if failed</returns>
        Task<UserEntity?> UpdateUserAsync(UserEntity user);

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="userId">User ID to update password for</param>
        /// <param name="passwordHash">New password hash</param>
        /// <param name="passwordSalt">New password salt</param>
        /// <returns>True if updated successfully, false otherwise</returns>
        Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash, string passwordSalt);
    }
}
