using System.Diagnostics;
using System.Diagnostics.Metrics;
using SampleProject.Infrastructure.Interfaces;
using Common.Options;
using Microsoft.Extensions.Options;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Advanced metrics service for comprehensive monitoring
    /// </summary>
    public class AdvancedMetricsService : IAdvancedMetricsService
    {
        private readonly Counter<long> _authenticationCounter;
        private readonly Counter<long> _businessEventCounter;
        private readonly Gauge<double> _cpuUsage;
        private readonly Histogram<double> _databaseQueryDuration;
        private readonly Counter<long> _errorCounter;
        private readonly ILogger<AdvancedMetricsService> _logger;
        private readonly Gauge<double> _memoryUsage;
        private readonly Meter _meter;
        private readonly MonitoringOptions _options;
        private readonly Counter<long> _requestCounter;
        private readonly Histogram<double> _requestDuration;

        /// <summary>
        /// Initializes a new instance of the AdvancedMetricsService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">Monitoring options</param>
        public AdvancedMetricsService(ILogger<AdvancedMetricsService> logger, IOptions<MonitoringOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _meter = new Meter(StringMessages.AdvancedMetricsServiceMeterName, StringMessages.AdvancedMetricsServiceVersion);

            // Request metrics
            _requestCounter = _meter.CreateCounter<long>(StringMessages.HttpRequestsTotalMetric, StringMessages.HttpRequestsTotalDescription);
            _errorCounter = _meter.CreateCounter<long>(StringMessages.HttpErrorsTotalMetric, StringMessages.HttpErrorsTotalDescription);
            _requestDuration = _meter.CreateHistogram<double>(StringMessages.HttpRequestDurationMetric, StringMessages.HttpRequestDurationDescription);

            // Authentication metrics
            _authenticationCounter = _meter.CreateCounter<long>(StringMessages.AuthenticationEventsTotalMetric, StringMessages.AuthenticationEventsTotalDescription);

            // Business metrics
            _businessEventCounter = _meter.CreateCounter<long>(StringMessages.BusinessEventsTotalMetric, StringMessages.BusinessEventsTotalDescription);

            // Database metrics
            _databaseQueryDuration = _meter.CreateHistogram<double>(StringMessages.DatabaseQueryDurationMetric, StringMessages.DatabaseQueryDurationDescription);

            // System metrics
            _memoryUsage = _meter.CreateGauge<double>(StringMessages.SystemMemoryUsageMetric, StringMessages.SystemMemoryUsageDescription);
            _cpuUsage = _meter.CreateGauge<double>(StringMessages.SystemCpuUsageMetric, StringMessages.SystemCpuUsageDescription);

            _logger.LogInformation(StringMessages.AdvancedMetricsServiceInitialized);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _meter?.Dispose();
            _logger.LogInformation(StringMessages.AdvancedMetricsServiceDisposed);
        }

        /// <inheritdoc />
        public void RecordAuthenticationEvent(string eventType, string userId, bool success, string? correlationId = null)
        {
            if (!_options.Enabled || !_options.EnableBusinessMetrics)
                return;

            var tags = new TagList
            {
                { StringMessages.EventTypeTag, eventType },
                { StringMessages.UserIdTag, userId },
                { StringMessages.SuccessTag, success.ToString() },
                { StringMessages.CorrelationIdTag, correlationId ?? StringMessages.UnknownValue }
            };

            _authenticationCounter.Add(1, tags);

            _logger.LogDebug(StringMessages.RecordedAuthenticationEvent, eventType, userId, success);
        }

        /// <inheritdoc />
        public void RecordBusinessEvent(string eventType, string entityType, string? entityId = null, string? correlationId = null)
        {
            if (!_options.Enabled || !_options.EnableBusinessEventTracking)
                return;

            var tags = new TagList
            {
                { StringMessages.EventTypeTag, eventType },
                { StringMessages.EntityTypeTag, entityType },
                { StringMessages.EntityIdTag, entityId ?? StringMessages.UnknownValue },
                { StringMessages.CorrelationIdTag, correlationId ?? StringMessages.UnknownValue }
            };

            _businessEventCounter.Add(1, tags);

            _logger.LogDebug(StringMessages.RecordedBusinessEvent, eventType, entityType, entityId);
        }

        /// <inheritdoc />
        public void RecordCpuUsage(double cpuPercentage)
        {
            if (!_options.Enabled || !_options.EnableCpuMonitoring)
                return;

            _cpuUsage.Record(cpuPercentage);

            _logger.LogDebug(StringMessages.RecordedCpuUsage, cpuPercentage);
        }

        /// <inheritdoc />
        public void RecordCustomMetric(string metricName, double value, Dictionary<string, string>? tags = null, string? correlationId = null)
        {
            if (!_options.Enabled || !_options.EnableCustomMetrics)
                return;

            var tagList = new TagList();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    tagList.Add(tag.Key, tag.Value);
                }
            }
            tagList.Add(StringMessages.CorrelationIdTag, correlationId ?? StringMessages.UnknownValue);

            // Create a custom counter for this metric
            var counter = _meter.CreateCounter<double>(metricName, StringMessages.CustomMetricDescription);
            counter.Add(value, tagList);

            _logger.LogDebug(StringMessages.RecordedCustomMetric, metricName, value);
        }

        /// <inheritdoc />
        public void RecordDatabaseQuery(string queryType, double durationSeconds, bool success, string? correlationId = null)
        {
            if (!_options.Enabled || !_options.EnableDatabaseMonitoring)
                return;

            var tags = new TagList
            {
                { StringMessages.QueryTypeTag, queryType },
                { StringMessages.SuccessTag, success.ToString() },
                { StringMessages.CorrelationIdTag, correlationId ?? StringMessages.UnknownValue }
            };

            _databaseQueryDuration.Record(durationSeconds, tags);

            _logger.LogDebug(StringMessages.RecordedDatabaseQuery, queryType, durationSeconds, success);
        }

        /// <inheritdoc />
        public void RecordMemoryUsage(long bytesUsed)
        {
            if (!_options.Enabled || !_options.EnableMemoryMonitoring)
                return;

            _memoryUsage.Record(bytesUsed);

            _logger.LogDebug(StringMessages.RecordedMemoryUsage, bytesUsed);
        }

        /// <inheritdoc />
        public void RecordRequest(string method, string path, int statusCode, double durationSeconds, string? correlationId = null)
        {
            if (!_options.Enabled || !_options.EnablePerformanceMetrics)
                return;

            var tags = new TagList
            {
                { StringMessages.MethodTag, method },
                { StringMessages.PathTag, path },
                { StringMessages.StatusCodeTag, statusCode.ToString() },
                { StringMessages.CorrelationIdTag, correlationId ?? StringMessages.UnknownValue }
            };

            _requestCounter.Add(1, tags);
            _requestDuration.Record(durationSeconds, tags);

            if (statusCode >= 400)
            {
                _errorCounter.Add(1, tags);
            }

            _logger.LogDebug(StringMessages.RecordedRequestMetrics, method, path, statusCode, durationSeconds);
        }
    }
}