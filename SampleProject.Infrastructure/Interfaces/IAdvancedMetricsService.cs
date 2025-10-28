namespace SampleProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for advanced metrics collection and monitoring
    /// </summary>
    public interface IAdvancedMetricsService : IDisposable
    {
        /// <summary>
        /// Records HTTP request metrics
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="path">Request path</param>
        /// <param name="statusCode">Response status code</param>
        /// <param name="durationSeconds">Request duration in seconds</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        void RecordRequest(string method, string path, int statusCode, double durationSeconds, string? correlationId = null);

        /// <summary>
        /// Records authentication event metrics
        /// </summary>
        /// <param name="eventType">Type of authentication event (login, logout, refresh, etc.)</param>
        /// <param name="userId">User ID</param>
        /// <param name="success">Whether the event was successful</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        void RecordAuthenticationEvent(string eventType, string userId, bool success, string? correlationId = null);

        /// <summary>
        /// Records business event metrics
        /// </summary>
        /// <param name="eventType">Type of business event (user_created, user_updated, etc.)</param>
        /// <param name="entityType">Type of entity affected</param>
        /// <param name="entityId">ID of the entity</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        void RecordBusinessEvent(string eventType, string entityType, string? entityId = null, string? correlationId = null);

        /// <summary>
        /// Records database query metrics
        /// </summary>
        /// <param name="queryType">Type of database query (select, insert, update, delete)</param>
        /// <param name="durationSeconds">Query duration in seconds</param>
        /// <param name="success">Whether the query was successful</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        void RecordDatabaseQuery(string queryType, double durationSeconds, bool success, string? correlationId = null);

        /// <summary>
        /// Records memory usage metrics
        /// </summary>
        /// <param name="bytesUsed">Memory usage in bytes</param>
        void RecordMemoryUsage(long bytesUsed);

        /// <summary>
        /// Records CPU usage metrics
        /// </summary>
        /// <param name="cpuPercentage">CPU usage percentage</param>
        void RecordCpuUsage(double cpuPercentage);

        /// <summary>
        /// Records custom metrics
        /// </summary>
        /// <param name="metricName">Name of the custom metric</param>
        /// <param name="value">Metric value</param>
        /// <param name="tags">Additional tags for the metric</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        void RecordCustomMetric(string metricName, double value, Dictionary<string, string>? tags = null, string? correlationId = null);
    }
}
