using System.ComponentModel.DataAnnotations;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Dto
{
    /// <summary>
    /// Request DTO for updating user information
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// User's active status (Admin only)
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// User's email verification status (Admin only)
        /// </summary>
        public bool? IsEmailVerified { get; set; }

        /// <summary>
        /// User's role (Admin only)
        /// </summary>
        public UserRole? Role { get; set; }
    }
}
