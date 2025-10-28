namespace Common.Options
{
    /// <summary>
    /// JWT authentication options
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// JWT secret key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// JWT issuer
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT audience
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Token expiration time in minutes (alias for ExpirationMinutes)
        /// </summary>
        public int ExpiryInMinutes 
        { 
            get => ExpirationMinutes; 
            set => ExpirationMinutes = value; 
        }

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Refresh token expiration time in days (alias for RefreshTokenExpirationDays)
        /// </summary>
        public int RefreshTokenExpiryInDays 
        { 
            get => RefreshTokenExpirationDays; 
            set => RefreshTokenExpirationDays = value; 
        }

        /// <summary>
        /// Enable JWT storage in HTTP-only cookies
        /// </summary>
        public bool UseCookies { get; set; } = true;

        /// <summary>
        /// Cookie name for access token
        /// </summary>
        public string AccessTokenCookieName { get; set; } = "auth_session";

        /// <summary>
        /// Cookie name for refresh token
        /// </summary>
        public string RefreshTokenCookieName { get; set; } = "auth_refresh";

        /// <summary>
        /// Cookie domain (null for current domain)
        /// </summary>
        public string? CookieDomain { get; set; }

        /// <summary>
        /// Cookie path
        /// </summary>
        public string CookiePath { get; set; } = "/";

        /// <summary>
        /// Enable secure cookies (HTTPS only)
        /// </summary>
        public bool SecureCookies { get; set; } = true;

        /// <summary>
        /// SameSite cookie attribute (Strict, Lax, None)
        /// </summary>
        public string SameSiteMode { get; set; } = "Strict";
    }
}
