namespace Common.Options
{
    /// <summary>
    /// Database connection options
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Connection string for the database
        /// </summary>
        private string _connectionString = string.Empty;

        /// <summary>
        /// Database provider (SqlServer, PostgreSQL, etc.)
        /// </summary>
        public string Provider { get; set; } = "SqlServer";

        /// <summary>
        /// Command timeout in seconds
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Enable sensitive data logging
        /// </summary>
        public bool EnableSensitiveDataLogging { get; set; } = false;

        /// <summary>
        /// Use in-memory database for testing
        /// </summary>
        public bool UseInMemory { get; set; } = false;

        /// <summary>
        /// Database name for in-memory database
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Connection string for the database
        /// </summary>
        public string ConnectionString 
        { 
            get => _connectionString; 
            set => _connectionString = value; 
        }
    }
}
