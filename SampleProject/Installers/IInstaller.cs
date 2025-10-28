namespace SampleProject.Installers
{
    /// <summary>
    /// Interface for service installers
    /// </summary>
    public interface IInstaller
    {
        /// <summary>
        /// Gets the installation order priority (lower numbers are installed first)
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Installs services into the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        void InstallServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Configures services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        void ConfigureServices(WebApplication app);
    }
}
