using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Shared;
using System.Reflection;

namespace SampleProject.Installers.InstallServices.Persistence
{
    /// <summary>
    /// Installer for persistence services
    /// </summary>
    public class PersistenceServiceInstaller : IInstaller
    {
        /// <summary>
        /// Order of installation (higher numbers install later)
        /// </summary>
        public int Order => 30;

        /// <summary>
        /// Installs persistence services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                // Use reflection to call ServicePersistanceInstaller.InstallPersistanceServices
                var persistenceAssembly = Assembly.Load("SampleProject.Persistence");
                var persistenceInstallerType = persistenceAssembly.GetType("SampleProject.Persistence.ServicePersistanceInstaller");
                
                if (persistenceInstallerType != null)
                {
                    var installMethod = persistenceInstallerType.GetMethod("InstallPersistanceServices", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (installMethod != null)
                    {
                        installMethod.Invoke(null, new object[] { services, configuration });
                        Log.Information("Persistence services installed successfully");
                    }
                    else
                    {
                        Log.Warning("InstallPersistanceServices method not found in ServicePersistanceInstaller");
                    }
                }
                else
                {
                    Log.Warning("ServicePersistanceInstaller type not found in SampleProject.Persistence assembly");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to install persistence services");
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
