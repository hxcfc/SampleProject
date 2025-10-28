namespace Common.Options
{
    /// <summary>
    /// Configuration options for database seeding
    /// </summary>
    public class DatabaseSeedingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether database seeding is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to seed sample data
        /// </summary>
        public bool SeedSampleData { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to seed admin user
        /// </summary>
        public bool SeedAdminUser { get; set; } = true;

        /// <summary>
        /// Gets or sets the admin user email
        /// </summary>
        public string AdminEmail { get; set; } = "admin@sampleproject.com";

        /// <summary>
        /// Gets or sets the admin user password
        /// </summary>
        public string AdminPassword { get; set; } = "Admin123!";

        /// <summary>
        /// Gets or sets the admin user first name
        /// </summary>
        public string AdminFirstName { get; set; } = "Admin";

        /// <summary>
        /// Gets or sets the admin user last name
        /// </summary>
        public string AdminLastName { get; set; } = "User";

        /// <summary>
        /// Gets or sets a value indicating whether to clear existing data before seeding
        /// </summary>
        public bool ClearExistingData { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of sample users to create
        /// </summary>
        public int SampleUsersCount { get; set; } = 10;
    }
}
