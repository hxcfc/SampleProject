using Common.Shared;
using System.Diagnostics;
using System.Text;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the RequestLoggingMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // Log incoming request
            await LogIncomingRequest(context, requestId);

            // Enable response body reading
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Preload any existing response content (e.g., set by previous middleware)
            try
            {
                if (originalResponseBodyStream.CanSeek && originalResponseBodyStream.Length > 0)
                {
                    originalResponseBodyStream.Seek(0, SeekOrigin.Begin);
                    await originalResponseBodyStream.CopyToAsync(responseBodyStream);
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception preloadEx) when (preloadEx is not OperationCanceledException && preloadEx is not TaskCanceledException)
            {
                _logger.LogDebug(preloadEx, "Failed to preload existing response body for request {RequestId}", requestId);
            }

            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                // Client disconnected (includes TaskCanceledException) - don't log, just rethrow
                _logger.LogDebug("Request {RequestId} cancelled", requestId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing request {RequestId}", requestId);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Don't try to read response if request was cancelled
                if (!context.RequestAborted.IsCancellationRequested)
                {
                    // Read response body before disposing the stream
                    string? responseBody = null;
                    if (context.Response.StatusCode >= 400)
                    {
                        try
                        {
                            responseBodyStream.Seek(0, SeekOrigin.Begin);
                            responseBody = await new StreamReader(responseBodyStream, Encoding.UTF8).ReadToEndAsync();
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException && ex is not TaskCanceledException)
                        {
                            _logger.LogWarning(ex, "Failed to read response body for request {RequestId}", requestId);
                        }
                    }

                    // Log outgoing response
                    LogOutgoingResponse(context, requestId, stopwatch.ElapsedMilliseconds, responseBody);

                    // Copy response back to original stream
                    try
                    {
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream already disposed, ignore
                        _logger.LogDebug("Response stream already disposed for request {RequestId}", requestId);
                    }
                    catch (NotSupportedException nse)
                    {
                        // Destination stream not expandable (e.g., fixed-size MemoryStream in tests)
                        _logger.LogDebug(nse, "Original response stream not expandable for request {RequestId}", requestId);
                    }
                }
            }
        }

        /// <summary>
        /// Logs incoming request details
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="requestId">Request ID</param>
        private async Task LogIncomingRequest(HttpContext context, string requestId)
        {
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var queryString = request.QueryString.ToString();
            var userAgent = request.Headers.UserAgent.ToString();
            var clientIp = GetClientIpAddress(context);

            _logger.LogInformation(
                "Request {RequestId}: {Method} {Path}{QueryString} from {ClientIp} - UserAgent: {UserAgent}",
                requestId, method, path, queryString, clientIp, userAgent);

            // Log request headers
            if (request.Headers.Any())
            {
                _logger.LogTrace("Request {RequestId} Headers: {@Headers}", requestId, 
                    request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
            }

            // Log request body for POST/PUT/PATCH requests
            if (ShouldLogRequestBody(request.Method))
            {
                try
                {
                    request.EnableBuffering();
                    var body = await new StreamReader(request.Body, Encoding.UTF8).ReadToEndAsync();
                    request.Body.Position = 0;

                    if (!string.IsNullOrEmpty(body))
                    {
                        _logger.LogInformation("Request {RequestId} Body: {Body}", requestId, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read request body for request {RequestId}", requestId);
                }
            }
        }

        /// <summary>
        /// Logs outgoing response details
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
        /// <param name="responseBody">Response body content (for error responses)</param>
        private void LogOutgoingResponse(HttpContext context, string requestId, long elapsedMilliseconds, string? responseBody = null)
        {
            var response = context.Response;
            var statusCode = response.StatusCode;
            var contentLength = response.ContentLength ?? 0;

            _logger.LogInformation(
                "Response {RequestId}: {StatusCode} in {ElapsedMs}ms - ContentLength: {ContentLength}",
                requestId, statusCode, elapsedMilliseconds, contentLength);

            // Log response headers
            if (response.Headers.Any())
            {
                _logger.LogTrace("Response {RequestId} Headers: {@Headers}", requestId,
                    response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
            }

            // Log response body for error responses (but skip static files and common client errors)
            if (statusCode >= 400 && !string.IsNullOrEmpty(responseBody))
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                
                // Don't log errors for static files (favicon, images, etc.)
                if (!path.Contains("favicon") && 
                    !path.Contains(".ico") && 
                    !path.Contains(".png") && 
                    !path.Contains(".jpg") && 
                    !path.Contains(".css") && 
                    !path.Contains(".js") &&
                    !path.Contains("_framework") &&
                    !path.Contains("_vs"))
                {
                    _logger.LogInformation("Response {RequestId} Error Body: {Body}", requestId, responseBody);
                }
            }
        }

        /// <summary>
        /// Determines if request body should be logged
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <returns>True if body should be logged</returns>
        private static bool ShouldLogRequestBody(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets client IP address from request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Client IP address</returns>
        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for real IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
