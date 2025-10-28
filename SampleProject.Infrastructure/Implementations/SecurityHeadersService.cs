using Common.Shared.Interfaces;
using Common.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Security headers service implementation
    /// </summary>
    public class SecurityHeadersService : ISecurityHeadersService
    {
        private readonly ILogger<SecurityHeadersService> _logger;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersService(IOptions<SecurityHeadersOptions> options, ILogger<SecurityHeadersService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void ApplySecurityHeaders(object responseObj)
        {
            try
            {
                if (responseObj is Microsoft.AspNetCore.Http.HttpResponse response)
                {
                    ApplyBasicSecurityHeaders(response);
                    ApplyContentSecurityPolicy(response);
                    ApplyTransportSecurityHeaders(response);
                    ApplyPermissionsPolicy(response);
                    ApplyCustomHeaders(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying security headers");
            }
        }

        private void ApplyBasicSecurityHeaders(HttpResponse response)
        {
            if (_options.EnableXContentTypeOptions)
            {
                response.Headers["X-Content-Type-Options"] = "nosniff";
            }

            if (_options.EnableXFrameOptions)
            {
                response.Headers["X-Frame-Options"] = "DENY";
            }

            if (_options.EnableXSSProtection)
            {
                response.Headers["X-XSS-Protection"] = "1; mode=block";
            }

            if (_options.EnableReferrerPolicy)
            {
                response.Headers["Referrer-Policy"] = "no-referrer";
            }
        }

        private void ApplyContentSecurityPolicy(HttpResponse response)
        {
            if (_options.EnableContentSecurityPolicy)
            {
                // Check if this is a Swagger request
                var isSwaggerRequest = response.HttpContext.Request.Path.StartsWithSegments("/swagger");
                
                if (isSwaggerRequest)
                {
                    // More permissive CSP for Swagger UI
                    response.Headers["Content-Security-Policy"] =
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                        "img-src 'self' data: https: blob:; " +
                        "font-src 'self' data: https:; " +
                        "connect-src 'self' ws: wss:; " +
                        "frame-ancestors 'none'; " +
                        "base-uri 'self'; " +
                        "form-action 'self'";
                }
                else
                {
                    // Standard CSP for other requests
                    response.Headers["Content-Security-Policy"] =
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                        "style-src 'self' 'unsafe-inline'; " +
                        "img-src 'self' data: https:; " +
                        "font-src 'self' data:; " +
                        "connect-src 'self'; " +
                        "frame-ancestors 'none'; " +
                        "base-uri 'self'; " +
                        "form-action 'self'";
                }
            }
        }

        private void ApplyCustomHeaders(HttpResponse response)
        {
            if (_options.CustomHeaders != null)
            {
                foreach (var header in _options.CustomHeaders)
                {
                    response.Headers[header.Key] = header.Value;
                }
            }
        }

        private void ApplyPermissionsPolicy(HttpResponse response)
        {
            if (_options.EnablePermissionsPolicy)
            {
                response.Headers["Permissions-Policy"] =
                    "accelerometer=(), " +
                    "camera=(), " +
                    "geolocation=(), " +
                    "gyroscope=(), " +
                    "magnetometer=(), " +
                    "microphone=(), " +
                    "payment=(), " +
                    "usb=()";
            }
        }

        private void ApplyTransportSecurityHeaders(HttpResponse response)
        {
            if (_options.EnableStrictTransportSecurity && response.HttpContext.Request.IsHttps)
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }
        }
    }
}