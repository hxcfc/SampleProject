using Prometheus;
using SampleProject.Infrastructure.Implementations;
using SampleProject.Infrastructure.Interfaces;

namespace SampleProject.Installers.InstallServices.Monitoring
{
    /// <summary>
    /// Installer for Prometheus metrics services
    /// </summary>
    public class PrometheusInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 10;

        /// <summary>
        /// Installs Prometheus metrics services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register custom metrics service
            services.AddSingleton<IMetricsService, MetricsService>();

            Log.Information(StringMessages.PrometheusMetricsServicesConfiguredSuccessfully);
        }

        /// <summary>
        /// Configures Prometheus metrics in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // Enable Prometheus metrics endpoint
            app.UseMetricServer();
            
            // Enable Prometheus metrics for HTTP requests
            app.UseHttpMetrics();

            Log.Information(StringMessages.PrometheusMetricsMiddlewareConfigured);
        }
    }
}
