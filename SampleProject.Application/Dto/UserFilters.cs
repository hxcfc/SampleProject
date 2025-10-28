using SampleProject.Domain.Enums;

namespace SampleProject.Application.Dto
{
    /// <summary>
    /// Filters for user search and listing
    /// </summary>
    public class UserFilters
    {
        /// <summary>
        /// Search term for first name or last name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by user role
        /// </summary>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filter by email verification status
        /// </summary>
        public bool? IsEmailVerified { get; set; }

        /// <summary>
        /// Filter by creation date from
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Filter by creation date to
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Sort field
        /// </summary>
        public string? SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string? SortDirection { get; set; } = "desc";
    }
}
