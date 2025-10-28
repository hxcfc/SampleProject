namespace Common.Options
{
    /// <summary>
    /// Health check configuration options
    /// </summary>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Enable health checks
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Enable health check UI
        /// </summary>
        public bool EnableUI { get; set; } = true;

        /// <summary>
        /// Health check endpoint path
        /// </summary>
        public string EndpointPath { get; set; } = "/health";

        /// <summary>
        /// Health check UI endpoint path
        /// </summary>
        public string UIEndpointPath { get; set; } = "/health-ui";

        /// <summary>
        /// Enable detailed responses
        /// </summary>
        public bool EnableDetailedResponses { get; set; } = true;

        /// <summary>
        /// Health check timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable database health check
        /// </summary>
        public bool EnableDatabaseHealthCheck { get; set; } = true;

        /// <summary>
        /// Enable memory health check
        /// </summary>
        public bool EnableMemoryHealthCheck { get; set; } = true;

        /// <summary>
        /// Memory threshold in MB
        /// </summary>
        public long MemoryThresholdMB { get; set; } = 1024;
    }
}
