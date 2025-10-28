namespace SampleProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for custom application metrics
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Records HTTP request metrics
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="endpoint">Endpoint path</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="duration">Request duration in seconds</param>
        void RecordHttpRequest(string method, string endpoint, int statusCode, double duration);

        /// <summary>
        /// Records login attempt metrics
        /// </summary>
        /// <param name="success">Whether login was successful</param>
        void RecordLoginAttempt(bool success);

        /// <summary>
        /// Records token refresh metrics
        /// </summary>
        /// <param name="success">Whether token refresh was successful</param>
        void RecordTokenRefresh(bool success);

        /// <summary>
        /// Records database query metrics
        /// </summary>
        /// <param name="operation">Database operation (SELECT, INSERT, UPDATE, DELETE)</param>
        /// <param name="table">Table name</param>
        /// <param name="duration">Query duration in seconds</param>
        void RecordDatabaseQuery(string operation, string table, double duration);

        /// <summary>
        /// Records user creation metrics
        /// </summary>
        void RecordUserCreated();

        /// <summary>
        /// Sets the number of active users
        /// </summary>
        /// <param name="count">Number of active users</param>
        void SetActiveUsers(int count);

        /// <summary>
        /// Sets the number of active database connections
        /// </summary>
        /// <param name="count">Number of active database connections</param>
        void SetDatabaseConnections(int count);
    }
}
