using Common.Options;

namespace SampleProject.Installers.InstallServices.Configuration
{
    /// <summary>
    /// Installer for configuration options
    /// </summary>
    public class OptionsInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 1;
        /// <summary>
        /// Installs configuration options
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration options
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
            
            // Configure JWT options - combine environment variables (sensitive data) with appsettings (technical config)
            services.Configure<JwtOptions>(options =>
            {
                // Sensitive data from environment variables
                options.SecretKey = configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
                options.Issuer = configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER environment variable is required");
                options.Audience = configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable is required");
                
                // Technical configuration from appsettings.json
                options.ExpirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
                options.RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
                options.UseCookies = configuration.GetValue<bool>("Jwt:UseCookies", true);
                options.AccessTokenCookieName = configuration.GetValue<string>("Jwt:AccessTokenCookieName", "auth_session");
                options.RefreshTokenCookieName = configuration.GetValue<string>("Jwt:RefreshTokenCookieName", "auth_refresh");
                options.CookieDomain = configuration.GetValue<string>("Jwt:CookieDomain");
                options.CookiePath = configuration.GetValue<string>("Jwt:CookiePath", "/");
                options.SecureCookies = configuration.GetValue<bool>("Jwt:SecureCookies", true);
                options.SameSiteMode = configuration.GetValue<string>("Jwt:SameSiteMode", "Strict");
            });
            
            services.Configure<SwaggerOptions>(configuration.GetSection("Swagger"));
            services.Configure<Common.Options.HealthCheckOptions>(configuration.GetSection("HealthCheck"));
            services.Configure<SerilogOptions>(configuration.GetSection("Serilog"));
            services.Configure<SecurityHeadersOptions>(configuration.GetSection("SecurityHeaders"));

            Log.Information(StringMessages.ConfigurationOptionsRegisteredSuccessfully);
        }

        /// <summary>
        /// Configures options services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            Log.Information(StringMessages.ConfigurationOptionsConfigured);
        }
    }
}
