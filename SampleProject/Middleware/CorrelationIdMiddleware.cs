using Microsoft.Extensions.Options;
using Serilog.Context;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Extension methods for correlation ID middleware
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Adds correlation ID middleware to the pipeline
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }

    /// <summary>
    /// Middleware for correlation ID tracking
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly MonitoringOptions _options;

        /// <summary>
        /// Initializes a new instance of the CorrelationIdMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">Monitoring options</param>
        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger, IOptions<MonitoringOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled || !_options.EnableCorrelationId)
            {
                await _next(context);
                return;
            }

            var correlationId = GetOrCreateCorrelationId(context);

            // Add correlation ID to response headers
            context.Response.Headers.Add(_options.CorrelationIdHeaderName, correlationId);

            // Add correlation ID to log context
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogDebug(StringMessages.ProcessingRequestWithCorrelationId, correlationId);
                await _next(context);
            }
        }

        /// <summary>
        /// Gets or creates a correlation ID for the request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Correlation ID</returns>
        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Try to get correlation ID from request headers
            if (context.Request.Headers.TryGetValue(_options.CorrelationIdHeaderName, out var headerValue) &&
                !string.IsNullOrEmpty(headerValue))
            {
                return headerValue.ToString();
            }

            // Generate new correlation ID
            var correlationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug(StringMessages.GeneratedNewCorrelationId, correlationId);

            return correlationId;
        }
    }
}