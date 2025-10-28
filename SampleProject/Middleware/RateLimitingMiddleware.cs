using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Extension methods for rate limiting middleware
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the pipeline
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }

    /// <summary>
    /// Rate limit information for a key
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        /// List of request timestamps
        /// </summary>
        public List<DateTime> Requests { get; set; } = new();
    }

    /// <summary>
    /// Middleware for rate limiting requests
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly RateLimitingOptions _options;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();

        /// <summary>
        /// Initializes a new instance of the RateLimitingMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">Rate limiting options</param>
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IOptions<RateLimitingOptions> options)
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
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);
            var endpoint = GetEndpointKey(context);

            // Check rate limits
            if (IsRateLimited(clientIp, endpoint))
            {
                await HandleRateLimitExceeded(context);
                return;
            }

            // Add rate limit headers
            if (_options.EnableRateLimitHeaders)
            {
                AddRateLimitHeaders(context, clientIp, endpoint);
            }

            await _next(context);
        }

        /// <summary>
        /// Gets the client IP address
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Client IP address</returns>
        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first
            var forwardedFor = context.Request.Headers[StringMessages.XForwardedForHeader].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for real IP
            var realIp = context.Request.Headers[StringMessages.XRealIpHeader].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? StringMessages.UnknownIpAddress;
        }

        /// <summary>
        /// Gets the endpoint key for rate limiting
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Endpoint key</returns>
        private static string GetEndpointKey(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Extract API endpoint from path
            if (path.StartsWith("/api/v"))
            {
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    return $"{parts[2]}/{parts[3]}"; // e.g., "auth/login"
                }
            }

            return path;
        }

        /// <summary>
        /// Handles rate limit exceeded scenario
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        private static async Task HandleRateLimitExceeded(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = StringMessages.RateLimitExceededMessage,
                retryAfter = int.Parse(StringMessages.RateLimitRetryAfterSeconds) // seconds
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Adds rate limit headers to the response
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="clientIp">Client IP address</param>
        /// <param name="endpoint">Endpoint key</param>
        private void AddRateLimitHeaders(HttpContext context, string clientIp, string endpoint)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-_options.WindowInMinutes);

            // Get current request counts
            var globalCount = GetRequestCount(StringMessages.GlobalRateLimitKey, windowStart);
            var ipCount = GetRequestCount(clientIp, windowStart);
            var endpointCount = GetRequestCount($"{clientIp}:{endpoint}", windowStart);

            // Add/overwrite headers (use indexer to avoid duplicate key exceptions across multiple calls)
            context.Response.Headers[StringMessages.XRateLimitLimitGlobalHeader] = _options.GlobalRateLimit.ToString();
            context.Response.Headers[StringMessages.XRateLimitRemainingGlobalHeader] = Math.Max(0, _options.GlobalRateLimit - globalCount).ToString();
            context.Response.Headers[StringMessages.XRateLimitLimitIpHeader] = _options.PerIpRateLimit.ToString();
            context.Response.Headers[StringMessages.XRateLimitRemainingIpHeader] = Math.Max(0, _options.PerIpRateLimit - ipCount).ToString();

            if (_options.EnableEndpointRateLimiting)
            {
                var endpointLimit = GetEndpointRateLimit(endpoint);
                if (endpointLimit > 0)
                {
                    context.Response.Headers[StringMessages.XRateLimitLimitEndpointHeader] = endpointLimit.ToString();
                    context.Response.Headers[StringMessages.XRateLimitRemainingEndpointHeader] = Math.Max(0, endpointLimit - endpointCount).ToString();
                }
            }

            context.Response.Headers[StringMessages.XRateLimitResetHeader] = now.AddMinutes(_options.WindowInMinutes).ToString("R");
        }

        /// <summary>
        /// Cleans up old entries from the rate limit store
        /// </summary>
        /// <param name="windowStart">Window start time</param>
        private void CleanupOldEntries(DateTime windowStart)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _rateLimitStore)
            {
                kvp.Value.Requests.RemoveAll(r => r < windowStart);
                if (kvp.Value.Requests.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _rateLimitStore.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Gets the rate limit for a specific endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint key</param>
        /// <returns>Rate limit for the endpoint</returns>
        private int GetEndpointRateLimit(string endpoint)
        {
            if (endpoint == StringMessages.AuthLoginEndpoint)
                return _options.AuthRateLimit;

            if (endpoint == StringMessages.AuthRefreshEndpoint)
                return _options.RefreshTokenRateLimit;

            return 0; // No specific limit
        }

        /// <summary>
        /// Gets the request count for a key within the window
        /// </summary>
        /// <param name="key">Rate limit key</param>
        /// <param name="windowStart">Window start time</param>
        /// <returns>Request count</returns>
        private int GetRequestCount(string key, DateTime windowStart)
        {
            if (_rateLimitStore.TryGetValue(key, out var rateLimitInfo))
            {
                return rateLimitInfo.Requests.Count(r => r >= windowStart);
            }
            return 0;
        }

        /// <summary>
        /// Checks if a specific key is rate limited
        /// </summary>
        /// <param name="key">Rate limit key</param>
        /// <param name="limit">Rate limit</param>
        /// <param name="now">Current time</param>
        /// <param name="windowStart">Window start time</param>
        /// <returns>True if rate limited</returns>
        private bool IsKeyRateLimited(string key, int limit, DateTime now, DateTime windowStart)
        {
            var rateLimitInfo = _rateLimitStore.AddOrUpdate(key,
                new RateLimitInfo { Requests = new List<DateTime> { now } },
                (k, existing) =>
                {
                    // Remove old requests outside the window
                    existing.Requests.RemoveAll(r => r < windowStart);

                    // Add current request
                    existing.Requests.Add(now);

                    return existing;
                });

            return rateLimitInfo.Requests.Count > limit;
        }

        /// <summary>
        /// Checks if the request is rate limited
        /// </summary>
        /// <param name="clientIp">Client IP address</param>
        /// <param name="endpoint">Endpoint key</param>
        /// <returns>True if rate limited</returns>
        private bool IsRateLimited(string clientIp, string endpoint)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-_options.WindowInMinutes);

            // Clean up old entries
            CleanupOldEntries(windowStart);

            // Check global rate limit
            var globalKey = StringMessages.GlobalRateLimitKey;
            if (IsKeyRateLimited(globalKey, _options.GlobalRateLimit, now, windowStart))
            {
                _logger.LogWarning(StringMessages.GlobalRateLimitExceeded, clientIp);
                return true;
            }

            // Check per-IP rate limit
            if (IsKeyRateLimited(clientIp, _options.PerIpRateLimit, now, windowStart))
            {
                _logger.LogWarning(StringMessages.PerIpRateLimitExceeded, clientIp);
                return true;
            }

            // Check endpoint-specific rate limits
            if (_options.EnableEndpointRateLimiting)
            {
                var endpointLimit = GetEndpointRateLimit(endpoint);
                if (endpointLimit > 0 && IsKeyRateLimited($"{clientIp}:{endpoint}", endpointLimit, now, windowStart))
                {
                    _logger.LogWarning(StringMessages.EndpointRateLimitExceeded, clientIp, endpoint);
                    return true;
                }
            }

            return false;
        }
    }
}