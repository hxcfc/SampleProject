using Prometheus;
using SampleProject.Infrastructure.Interfaces;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Service for custom application metrics
    /// </summary>
    public class MetricsService : IMetricsService
    {
        // HTTP Request Metrics
        private static readonly Counter HttpRequestsTotal = Metrics
            .CreateCounter(StringMessages.MetricsHttpRequestsTotal, StringMessages.MetricsHttpRequestsDescription, 
                new[] { StringMessages.MetricsLabelMethod, StringMessages.MetricsLabelEndpoint, StringMessages.MetricsLabelStatusCode });

        private static readonly Histogram HttpRequestDuration = Metrics
            .CreateHistogram(StringMessages.MetricsHttpRequestDuration, StringMessages.MetricsHttpRequestDurationDescription, 
                new[] { StringMessages.MetricsLabelMethod, StringMessages.MetricsLabelEndpoint });

        // Authentication Metrics
        private static readonly Counter LoginAttemptsTotal = Metrics
            .CreateCounter(StringMessages.MetricsLoginAttemptsTotal, StringMessages.MetricsLoginAttemptsDescription, 
                new[] { StringMessages.MetricsLabelResult });

        private static readonly Counter TokenRefreshesTotal = Metrics
            .CreateCounter(StringMessages.MetricsTokenRefreshesTotal, StringMessages.MetricsTokenRefreshesDescription, 
                new[] { StringMessages.MetricsLabelResult });

        // Database Metrics
        private static readonly Counter DatabaseQueriesTotal = Metrics
            .CreateCounter(StringMessages.MetricsDatabaseQueriesTotal, StringMessages.MetricsDatabaseQueriesDescription, 
                new[] { StringMessages.MetricsLabelOperation, StringMessages.MetricsLabelTable });

        private static readonly Histogram DatabaseQueryDuration = Metrics
            .CreateHistogram(StringMessages.MetricsDatabaseQueryDuration, StringMessages.MetricsDatabaseQueryDurationDescription, 
                new[] { StringMessages.MetricsLabelOperation, StringMessages.MetricsLabelTable });

        // Business Logic Metrics
        private static readonly Counter UsersCreatedTotal = Metrics
            .CreateCounter(StringMessages.MetricsUsersCreatedTotal, StringMessages.MetricsUsersCreatedDescription);

        private static readonly Gauge ActiveUsers = Metrics
            .CreateGauge(StringMessages.MetricsActiveUsers, StringMessages.MetricsActiveUsersDescription);

        private static readonly Gauge DatabaseConnections = Metrics
            .CreateGauge(StringMessages.MetricsDatabaseConnections, StringMessages.MetricsDatabaseConnectionsDescription);

        /// <summary>
        /// Records HTTP request metrics
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="endpoint">Endpoint path</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="duration">Request duration in seconds</param>
        public void RecordHttpRequest(string method, string endpoint, int statusCode, double duration)
        {
            HttpRequestsTotal.WithLabels(method, endpoint, statusCode.ToString()).Inc();
            HttpRequestDuration.WithLabels(method, endpoint).Observe(duration);
        }

        /// <summary>
        /// Records login attempt metrics
        /// </summary>
        /// <param name="success">Whether login was successful</param>
        public void RecordLoginAttempt(bool success)
        {
            var result = success ? StringMessages.MetricsResultSuccess : StringMessages.MetricsResultFailure;
            LoginAttemptsTotal.WithLabels(result).Inc();
        }

        /// <summary>
        /// Records token refresh metrics
        /// </summary>
        /// <param name="success">Whether token refresh was successful</param>
        public void RecordTokenRefresh(bool success)
        {
            var result = success ? StringMessages.MetricsResultSuccess : StringMessages.MetricsResultFailure;
            TokenRefreshesTotal.WithLabels(result).Inc();
        }

        /// <summary>
        /// Records database query metrics
        /// </summary>
        /// <param name="operation">Database operation (SELECT, INSERT, UPDATE, DELETE)</param>
        /// <param name="table">Table name</param>
        /// <param name="duration">Query duration in seconds</param>
        public void RecordDatabaseQuery(string operation, string table, double duration)
        {
            DatabaseQueriesTotal.WithLabels(operation, table).Inc();
            DatabaseQueryDuration.WithLabels(operation, table).Observe(duration);
        }

        /// <summary>
        /// Records user creation metrics
        /// </summary>
        public void RecordUserCreated()
        {
            UsersCreatedTotal.Inc();
        }

        /// <summary>
        /// Updates active users count
        /// </summary>
        /// <param name="count">Number of active users</param>
        public void SetActiveUsers(int count)
        {
            ActiveUsers.Set(count);
        }

        /// <summary>
        /// Updates database connections count
        /// </summary>
        /// <param name="count">Number of active database connections</param>
        public void SetDatabaseConnections(int count)
        {
            DatabaseConnections.Set(count);
        }
    }
}
