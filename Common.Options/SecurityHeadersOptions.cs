namespace Common.Options
{
    /// <summary>
    /// Security headers configuration options
    /// </summary>
    public class SecurityHeadersOptions
    {
        /// <summary>
        /// Enable X-Content-Type-Options header
        /// </summary>
        public bool EnableXContentTypeOptions { get; set; } = true;

        /// <summary>
        /// Enable X-Frame-Options header
        /// </summary>
        public bool EnableXFrameOptions { get; set; } = true;

        /// <summary>
        /// Enable X-XSS-Protection header
        /// </summary>
        public bool EnableXSSProtection { get; set; } = true;

        /// <summary>
        /// Enable Referrer-Policy header
        /// </summary>
        public bool EnableReferrerPolicy { get; set; } = true;

        /// <summary>
        /// Enable Content-Security-Policy header
        /// </summary>
        public bool EnableContentSecurityPolicy { get; set; } = true;

        /// <summary>
        /// Enable Permissions-Policy header
        /// </summary>
        public bool EnablePermissionsPolicy { get; set; } = true;

        /// <summary>
        /// Enable Strict-Transport-Security header
        /// </summary>
        public bool EnableStrictTransportSecurity { get; set; } = true;

        /// <summary>
        /// Custom headers to apply
        /// </summary>
        public Dictionary<string, string>? CustomHeaders { get; set; }
    }
}
