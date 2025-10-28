using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace SampleProject.Installers.InstallServices.Configuration
{
    /// <summary>
    /// Installer for API versioning services
    /// </summary>
    public class ApiVersioningInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 4;

        /// <summary>
        /// Installs API versioning services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add API versioning
            services.AddApiVersioning(options =>
            {
                // Default version when no version is specified
                options.DefaultApiVersion = new ApiVersion(1, 0);
                
                // Assume default version when versioning is not specified
                options.AssumeDefaultVersionWhenUnspecified = true;
                
                // Report API versions in response headers
                options.ReportApiVersions = true;
                
                // Use URL path versioning (e.g., /api/v1/controller)
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("ver")
                );
            });

            // Add API versioning explorer for Swagger
            services.AddVersionedApiExplorer(options =>
            {
                // Group by version
                options.GroupNameFormat = "'v'VVV";
                
                // Substitute version in URL
                options.SubstituteApiVersionInUrl = true;
            });

            // Configure API behavior
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disable automatic model state validation
                options.SuppressModelStateInvalidFilter = true;
            });

            Log.Information(StringMessages.ApiVersioningServicesConfiguredSuccessfully);
        }

        /// <summary>
        /// Configures API versioning services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // API versioning is configured in InstallServices
            Log.Information(StringMessages.ApiVersioningMiddlewareConfigured);
        }
    }
}
