using SampleProject.Infrastructure.Implementations;
using SampleProject.Infrastructure.Interfaces;
using Common.Options;

namespace SampleProject.Installers.InstallServices.Monitoring
{
    /// <summary>
    /// Installer for advanced monitoring services
    /// </summary>
    public class AdvancedMonitoringInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 11;

        /// <summary>
        /// Installs advanced monitoring services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure options
            services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
            services.Configure<MonitoringOptions>(configuration.GetSection("Monitoring"));

            // Register advanced metrics service
            services.AddSingleton<IAdvancedMetricsService, AdvancedMetricsService>();

            // Register system monitoring service
            services.AddSingleton<ISystemMonitoringService, SystemMonitoringService>();

            Log.Information(StringMessages.AdvancedMonitoringServicesConfigured);
        }

        /// <summary>
        /// No middleware registration here; middleware is ordered in Program.cs
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            Log.Information(StringMessages.AdvancedMonitoringServicesReady);
        }
    }
}
