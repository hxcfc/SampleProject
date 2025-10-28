using System.ComponentModel.DataAnnotations;
using SampleProject.Domain.Enums;

namespace SampleProject.Application.Dto
{
    /// <summary>
    /// Request DTO for changing user role
    /// </summary>
    public class ChangeUserRoleRequest
    {
        /// <summary>
        /// New user role
        /// </summary>
        [Required]
        public UserRole NewRole { get; set; }
    }
}
