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
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
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
