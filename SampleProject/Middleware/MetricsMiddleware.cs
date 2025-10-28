using System.Diagnostics;
using SampleProject.Infrastructure.Implementations;
using Prometheus;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Middleware for collecting custom application metrics
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsMiddleware> _logger;
        private static readonly Histogram _httpRequestDuration = Metrics
            .CreateHistogram("http_request_duration_seconds_custom", "Duration of HTTP requests in seconds.", new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.005, 2, 10), // 5ms to ~2.5s
                LabelNames = new[] { "method", "path", "status_code" }
            });

        /// <summary>
        /// Initializes a new instance of the MetricsMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = GetEndpointPath(context);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.Elapsed.TotalSeconds;
                var statusCode = context.Response.StatusCode;

                // Record metrics using Prometheus directly
                _httpRequestDuration.WithLabels(method, path, statusCode.ToString())
                    .Observe(duration);

                _logger.LogDebug(StringMessages.MetricsMiddlewareRecorded, 
                    method, path, statusCode, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Gets the endpoint path for metrics
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Endpoint path</returns>
        private static string GetEndpointPath(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";
            
            // Normalize path for metrics (remove IDs, etc.)
            if (path.Contains(StringMessages.MetricsMiddlewareApiPathPrefix))
            {
                // Extract API path pattern
                var apiIndex = path.IndexOf(StringMessages.MetricsMiddlewareApiPathPrefix, StringComparison.Ordinal);
                if (apiIndex >= 0)
                {
                    var apiPath = path.Substring(apiIndex);
                    
                    // Replace GUIDs and numbers with placeholders
                    apiPath = System.Text.RegularExpressions.Regex.Replace(apiPath, 
                        @"\/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", 
                        StringMessages.MetricsMiddlewareNormalizedPath);
                    apiPath = System.Text.RegularExpressions.Regex.Replace(apiPath, 
                        @"\/\d+", 
                        StringMessages.MetricsMiddlewareNormalizedPath);
                    
                    return apiPath;
                }
            }
            
            return path;
        }
    }
}
