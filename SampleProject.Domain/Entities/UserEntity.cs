using SampleProject.Domain.Enums;

namespace SampleProject.Domain.Entities
{
    /// <summary>
    /// User entity representing a user in the system
    /// </summary>
    [Table("Users")]
    public class UserEntity
    {
        /// <summary>
        /// Date and time when the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User's email address (unique)
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the user's full name
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Key]
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
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Salt for password hashing
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PasswordSalt { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for JWT authentication
        /// </summary>
        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Refresh token expiration date
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }

        /// <summary>
        /// Number of times the refresh token has been used
        /// </summary>
        public int RefreshTokenUseCount { get; set; } = 0;

        /// <summary>
        /// Date and time when the current refresh token was last used
        /// </summary>
        public DateTime? RefreshTokenLastUsedAt { get; set; }

        /// <summary>
        /// User roles as enum flags
        /// </summary>
        public UserRole Roles { get; set; } = UserRole.User;

        /// <summary>
        /// Date and time when the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}