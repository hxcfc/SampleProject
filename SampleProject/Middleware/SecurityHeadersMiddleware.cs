using Common.Shared.Interfaces;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Middleware for adding security headers
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecurityHeadersService _securityHeadersService;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the SecurityHeadersMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="securityHeadersService">Security headers service</param>
        /// <param name="logger">Logger instance</param>
        public SecurityHeadersMiddleware(RequestDelegate next, ISecurityHeadersService securityHeadersService, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _securityHeadersService = securityHeadersService;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Add security headers using the service
                _securityHeadersService.ApplySecurityHeaders(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorApplyingSecurityHeaders);
            }

            await _next(context);
        }
    }
}
