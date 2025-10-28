using SampleProject.Domain.Enums;
using SampleProject.Domain.Entities;

namespace SampleProject.Domain.Dto
{
    /// <summary>
    /// Data Transfer Object for User entity
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Date and time when the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Full name of the user (computed property)
        /// </summary>
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Unique identifier of the user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Indicates if the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indicates if the user email is verified
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// Date and time when the user last logged in
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// User roles as enum flags
        /// </summary>
        public UserRole Roles { get; set; } = UserRole.User;

        /// <summary>
        /// Date and time when the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Creates a UserDto from a UserEntity
        /// </summary>
        /// <param name="entity">User entity</param>
        /// <returns>User DTO</returns>
        public static UserDto FromEntity(UserEntity entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Roles = entity.Roles,
                IsActive = entity.IsActive,
                IsEmailVerified = entity.IsEmailVerified,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                LastLoginAt = entity.LastLoginAt
            };
        }
    }
}