using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Shared;
using System.Reflection;

namespace SampleProject.Installers.InstallServices.Infrastructure
{
    /// <summary>
    /// Installer for infrastructure services
    /// </summary>
    public class InfrastructureServiceInstaller : IInstaller
    {
        /// <summary>
        /// Order of installation (higher numbers install later)
        /// </summary>
        public int Order => 20;

        /// <summary>
        /// Installs infrastructure services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                // Use reflection to call InfrastructureInstaller.InstallInfrastructureServices
                var infrastructureAssembly = Assembly.Load("SampleProject.Infrastructure");
                var infrastructureInstallerType = infrastructureAssembly.GetType("SampleProject.Infrastructure.InfrastructureInstaller");
                
                if (infrastructureInstallerType != null)
                {
                    var installMethod = infrastructureInstallerType.GetMethod("InstallInfrastructureServices", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (installMethod != null)
                    {
                        installMethod.Invoke(null, new object[] { services, configuration });
                        Log.Information("Infrastructure services installed successfully");
                    }
                    else
                    {
                        Log.Warning("InstallInfrastructureServices method not found in InfrastructureInstaller");
                    }
                }
                else
                {
                    Log.Warning("InfrastructureInstaller type not found in SampleProject.Infrastructure assembly");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to install infrastructure services");
                throw;
            }
        }

        /// <summary>
        /// Configures services after application is built
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // No additional configuration needed
        }
    }
}
