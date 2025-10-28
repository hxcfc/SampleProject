using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Common.Shared;
using System.Reflection;

namespace SampleProject.Installers.InstallServices.Application
{
    /// <summary>
    /// Installer for application services
    /// </summary>
    public class ApplicationServiceInstaller : IInstaller
    {
        /// <summary>
        /// Order of installation (higher numbers install later)
        /// </summary>
        public int Order => 5;

        /// <summary>
        /// Installs application services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                // Use reflection to call ServiceApplicationInstaller.InstallApplicationServices
                var applicationAssembly = Assembly.Load("SampleProject.Application");
                var applicationInstallerType = applicationAssembly.GetType("SampleProject.Application.ServiceApplicationInstaller");
                
                if (applicationInstallerType != null)
                {
                    var installMethod = applicationInstallerType.GetMethod("InstallApplicationServices", 
                        BindingFlags.Public | BindingFlags.Static);
                    
                    if (installMethod != null)
                    {
                        installMethod.Invoke(null, new object[] { services, configuration });
                        Log.Information("Application services installed successfully");
                    }
                    else
                    {
                        Log.Warning("InstallApplicationServices method not found in ServiceApplicationInstaller");
                    }
                }
                else
                {
                    Log.Warning("ServiceApplicationInstaller type not found in SampleProject.Application assembly");
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to install application services");
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
