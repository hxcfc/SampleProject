using FluentValidation;

namespace SampleProject.Installers.InstallServices.Configuration
{
    /// <summary>
    /// Installer for FluentValidation services
    /// </summary>
    public class ValidationInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 3;
        /// <summary>
        /// Installs FluentValidation services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register FluentValidation
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            Log.Information(StringMessages.FluentValidationServicesRegisteredSuccessfully);
        }

        /// <summary>
        /// Configures FluentValidation services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            Log.Information(StringMessages.FluentValidationServicesConfigured);
        }
    }
}
