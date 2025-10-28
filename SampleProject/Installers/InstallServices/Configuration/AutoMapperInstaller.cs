namespace SampleProject.Installers.InstallServices.Configuration
{
    /// <summary>
    /// Installer for AutoMapper services
    /// </summary>
    public class AutoMapperInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 3;
        /// <summary>
        /// Installs AutoMapper services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register AutoMapper
            services.AddAutoMapper(typeof(Program));

            Log.Information(StringMessages.AutoMapperServicesRegisteredSuccessfully);
        }

        /// <summary>
        /// Configures AutoMapper services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            Log.Information(StringMessages.AutoMapperServicesConfigured);
        }
    }
}
