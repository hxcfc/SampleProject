using System.ComponentModel.DataAnnotations;

namespace SampleProject.Application.Dto
{
    /// <summary>
    /// Request DTO for updating current user's profile
    /// </summary>
    public class UpdateMyProfileRequest
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
    }
}
