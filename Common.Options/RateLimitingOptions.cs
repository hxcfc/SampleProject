namespace Common.Options
{
    /// <summary>
    /// Rate limiting configuration options
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Enable rate limiting
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Global rate limit (requests per minute)
        /// </summary>
        public int GlobalRateLimit { get; set; } = 1000;

        /// <summary>
        /// Per-IP rate limit (requests per minute)
        /// </summary>
        public int PerIpRateLimit { get; set; } = 100;

        /// <summary>
        /// Authentication endpoint rate limit (requests per minute)
        /// </summary>
        public int AuthRateLimit { get; set; } = 10;

        /// <summary>
        /// Refresh token rate limit (requests per minute)
        /// </summary>
        public int RefreshTokenRateLimit { get; set; } = 5;

        /// <summary>
        /// Rate limit window in minutes
        /// </summary>
        public int WindowInMinutes { get; set; } = 1;

        /// <summary>
        /// Enable rate limiting for specific endpoints
        /// </summary>
        public bool EnableEndpointRateLimiting { get; set; } = true;

        /// <summary>
        /// Rate limit policy name
        /// </summary>
        public string PolicyName { get; set; } = "RateLimitingPolicy";

        /// <summary>
        /// Rate limit message
        /// </summary>
        public string RateLimitMessage { get; set; } = "Rate limit exceeded. Please try again later.";

        /// <summary>
        /// Enable rate limit headers in response
        /// </summary>
        public bool EnableRateLimitHeaders { get; set; } = true;
    }
}
