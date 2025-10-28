using MediatR;
using SampleProject.Application;
using Common.Shared;

namespace SampleProject.Installers.InstallServices.Configuration
{
    /// <summary>
    /// Installer for MediatR services
    /// </summary>
    public class MediatRInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 7;
        /// <summary>
        /// Installs MediatR services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var programAssembly = typeof(Program).Assembly;
            var applicationAssembly = typeof(SampleProject.Application.ServiceApplicationInstaller).Assembly;
            
            // Register MediatR with Application assembly where handlers are located
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(programAssembly);
                cfg.RegisterServicesFromAssembly(applicationAssembly);
            });

            Log.Information(StringMessages.MediatRServicesRegisteredSuccessfully);
        }

        /// <summary>
        /// Configures MediatR services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // MediatR is configured and ready
            Log.Information(StringMessages.MediatRServicesConfigured);
        }
    }
}
