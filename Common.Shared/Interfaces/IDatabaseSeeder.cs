namespace Common.Shared.Interfaces
{
    /// <summary>
    /// Interface for database seeding operations
    /// </summary>
    public interface IDatabaseSeeder
    {
        /// <summary>
        /// Seeds initial data into the database
        /// </summary>
        /// <returns>Task representing the seeding operation</returns>
        Task SeedAsync();
    }
}
