namespace Common.Options
{
    /// <summary>
    /// Advanced monitoring configuration options
    /// </summary>
    public class MonitoringOptions
    {
        /// <summary>
        /// Enable advanced monitoring
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Enable business metrics collection
        /// </summary>
        public bool EnableBusinessMetrics { get; set; } = true;

        /// <summary>
        /// Enable performance metrics collection
        /// </summary>
        public bool EnablePerformanceMetrics { get; set; } = true;

        /// <summary>
        /// Enable custom metrics collection
        /// </summary>
        public bool EnableCustomMetrics { get; set; } = true;

        /// <summary>
        /// Metrics collection interval in seconds
        /// </summary>
        public int CollectionIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Enable correlation ID tracking
        /// </summary>
        public bool EnableCorrelationId { get; set; } = true;

        /// <summary>
        /// Correlation ID header name
        /// </summary>
        public string CorrelationIdHeaderName { get; set; } = "X-Correlation-ID";

        /// <summary>
        /// Enable request/response body logging for monitoring
        /// </summary>
        public bool EnableBodyLogging { get; set; } = false;

        /// <summary>
        /// Enable database query monitoring
        /// </summary>
        public bool EnableDatabaseMonitoring { get; set; } = true;

        /// <summary>
        /// Enable memory usage monitoring
        /// </summary>
        public bool EnableMemoryMonitoring { get; set; } = true;

        /// <summary>
        /// Enable CPU usage monitoring
        /// </summary>
        public bool EnableCpuMonitoring { get; set; } = true;

        /// <summary>
        /// Enable custom business event tracking
        /// </summary>
        public bool EnableBusinessEventTracking { get; set; } = true;

        /// <summary>
        /// Metrics retention period in days
        /// </summary>
        public int MetricsRetentionDays { get; set; } = 30;
    }
}
