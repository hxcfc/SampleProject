using Microsoft.AspNetCore.Http;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Extension methods for middleware registration
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds exception handling middleware
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application</returns>
        public static WebApplication UseExceptionHandling(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds request logging middleware
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application</returns>
        public static WebApplication UseRequestLogging(this WebApplication app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds security headers middleware
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application</returns>
        public static WebApplication UseSecurityHeaders(this WebApplication app)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds metrics middleware
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application</returns>
        public static WebApplication UseMetrics(this WebApplication app)
        {
            app.UseMiddleware<MetricsMiddleware>();
            return app;
        }
    }
}
