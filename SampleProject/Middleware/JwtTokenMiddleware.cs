using SampleProject.Infrastructure.Interfaces;

namespace SampleProject.Middleware
{
    /// <summary>
    /// Extension methods for registering JWT token middleware
    /// </summary>
    public static class JwtTokenMiddlewareExtensions
    {
        /// <summary>
        /// Adds JWT token middleware to the pipeline
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtTokenMiddleware>();
        }
    }

    /// <summary>
    /// Middleware for extracting JWT token from cookies or Authorization header
    /// and setting HttpContext.User with claims
    /// </summary>
    public class JwtTokenMiddleware
    {
        private readonly ILogger<JwtTokenMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the JwtTokenMiddleware class
        /// </summary>
        /// <param name="next">Next middleware in pipeline</param>
        /// <param name="logger">Logger instance</param>
        public JwtTokenMiddleware(RequestDelegate next, ILogger<JwtTokenMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="extendedJwtService">Extended JWT service</param>
        public async Task InvokeAsync(HttpContext context, IExtendedJwtService extendedJwtService)
        {
            try
            {
                // Skip token processing for anonymous endpoints
                if (IsAnonymousEndpoint(context))
                {
                    await _next(context);
                    return;
                }

                // Extract token from request
                var token = extendedJwtService.GetTokenFromRequest(context.Request);

                if (!string.IsNullOrEmpty(token))
                {
                    // Validate token and extract claims
                    var claims = ExtractClaimsFromToken(token, extendedJwtService);

                    if (claims.Any())
                    {
                        // Create ClaimsIdentity and set HttpContext.User
                        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                        context.User = new ClaimsPrincipal(identity);

                        _logger.LogDebug("JWT token processed successfully for user: {UserId}",
                            claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid or expired JWT token");
                    }
                }
                else
                {
                    _logger.LogDebug("No JWT token found in request");
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in JwtTokenMiddleware");
                // Continue processing without rethrowing to allow the request to proceed
                await _next(context);
            }
        }

        /// <summary>
        /// Extracts claims from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="extendedJwtService">Extended JWT service</param>
        /// <returns>List of claims</returns>
        private static List<Claim> ExtractClaimsFromToken(string token, IExtendedJwtService extendedJwtService)
        {
            try
            {
                var claims = new List<Claim>();

                // Validate token first
                if (!extendedJwtService.ValidateToken(token))
                {
                    return claims;
                }

                // Extract user information
                var userId = extendedJwtService.GetUserIdFromToken(token);
                var username = extendedJwtService.GetUsernameFromToken(token);
                var email = extendedJwtService.GetEmailFromToken(token);
                var firstName = extendedJwtService.GetFirstNameFromToken(token);
                var lastName = extendedJwtService.GetLastNameFromToken(token);
                var role = extendedJwtService.GetRoleFromToken(token);

                // Add standard claims
                if (!string.IsNullOrEmpty(userId))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId));
                }

                if (!string.IsNullOrEmpty(username))
                {
                    claims.Add(new Claim(ClaimTypes.Name, username));
                }

                if (!string.IsNullOrEmpty(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
                }

                if (!string.IsNullOrEmpty(firstName))
                {
                    claims.Add(new Claim(ClaimTypes.GivenName, firstName));
                    claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, firstName));
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    claims.Add(new Claim(ClaimTypes.Surname, lastName));
                    claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, lastName));
                }

                // Add role claims
                if (!string.IsNullOrEmpty(role))

                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                return claims;
            }
            catch (Exception)
            {
                return new List<Claim>();
            }
        }

        /// <summary>
        /// Checks if the current endpoint allows anonymous access
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if endpoint allows anonymous access</returns>
        private static bool IsAnonymousEndpoint(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
            {
                return true;
            }

            // Check for specific anonymous paths
            var path = context.Request.Path.Value?.ToLowerInvariant();
            return path switch
            {
                "/api/v1/auth/login" => true,
                "/api/v1/auth/refresh" => true,
                "/api/v1/users" => true, // User registration endpoint
                "/health" => true,
                "/health-ui" => true,
                "/swagger" => true,
                _ when path?.StartsWith("/swagger/") == true => true,
                _ => false
            };
        }
    }
}