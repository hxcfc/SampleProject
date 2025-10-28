namespace SampleProject.Domain.Entities
{
    /// <summary>
    /// Audit log entity for tracking user changes history
    /// </summary>
    [Table("UserAuditLogs")]
    public class UserAuditLog
    {
        /// <summary>
        /// Unique identifier for the audit log entry
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the user being audited
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Type of change (Created, Updated, Deleted, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Field that was changed (if applicable)
        /// </summary>
        [MaxLength(100)]
        public string? FieldName { get; set; }

        /// <summary>
        /// Old value before change
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after change
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// User who made the change (can be the same user or admin)
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        /// <summary>
        /// IP address where the change was made from
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser/client) where the change was made from
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Timestamp when the change occurred
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional notes or description
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
