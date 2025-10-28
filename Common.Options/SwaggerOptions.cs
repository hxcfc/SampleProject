namespace Common.Options
{
    /// <summary>
    /// Swagger configuration options
    /// </summary>
    public class SwaggerOptions
    {
        /// <summary>
        /// Swagger title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Swagger version
        /// </summary>
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Swagger description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Contact name
        /// </summary>
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Contact email
        /// </summary>
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Contact URL
        /// </summary>
        public string ContactUrl { get; set; } = string.Empty;

        /// <summary>
        /// License name
        /// </summary>
        public string LicenseName { get; set; } = string.Empty;

        /// <summary>
        /// License URL
        /// </summary>
        public string LicenseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Terms of service URL
        /// </summary>
        public string TermsOfServiceUrl { get; set; } = string.Empty;

        /// <summary>
        /// Enable XML comments
        /// </summary>
        public bool EnableXmlComments { get; set; } = true;

        /// <summary>
        /// Enable authentication
        /// </summary>
        public bool EnableAuthentication { get; set; } = true;

        /// <summary>
        /// Enable deep linking
        /// </summary>
        public bool EnableDeepLinking { get; set; } = true;

        /// <summary>
        /// Enable filter
        /// </summary>
        public bool EnableFilter { get; set; } = true;

        /// <summary>
        /// Show request duration
        /// </summary>
        public bool ShowRequestDuration { get; set; } = true;

        /// <summary>
        /// Swagger servers
        /// </summary>
        public List<SwaggerServerOptions> Servers { get; set; } = new();

        /// <summary>
        /// Enable Swagger in production
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Demo credentials for Swagger UI
        /// </summary>
        public SwaggerDemoCredentialsOptions DemoCredentials { get; set; } = new();

        /// <summary>
        /// Swagger UI authentication settings
        /// </summary>
        public SwaggerAuthOptions Auth { get; set; } = new();
    }

    /// <summary>
    /// Swagger server options
    /// </summary>
    public class SwaggerServerOptions
    {
        /// <summary>
        /// Server URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Server description
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Swagger demo credentials options
    /// </summary>
    public class SwaggerDemoCredentialsOptions
    {
        /// <summary>
        /// Demo username
        /// </summary>
        public string Username { get; set; } = "admin";

        /// <summary>
        /// Demo password
        /// </summary>
        public string Password { get; set; } = "admin";

        /// <summary>
        /// Enable demo credentials display
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// List of demo users
        /// </summary>
        public List<DemoUser> Users { get; set; } = new();
    }

    /// <summary>
    /// Demo user information
    /// </summary>
    public class DemoUser
    {
        /// <summary>
        /// Username/Email
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// User role/description
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Swagger UI authentication options
    /// </summary>
    public class SwaggerAuthOptions
    {
        /// <summary>
        /// Enable Swagger UI authentication
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Username for Swagger UI access
        /// </summary>
        public string Username { get; set; } = "swagger";

        /// <summary>
        /// Password for Swagger UI access
        /// </summary>
        public string Password { get; set; } = "swagger";
    }
}
