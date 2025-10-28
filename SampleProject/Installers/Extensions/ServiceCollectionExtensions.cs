using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Shared;

namespace SampleProject.Installers.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection to register services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Installs all services from the assembly in the correct order
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var installers = typeof(Program).Assembly.ExportedTypes
                    .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(Activator.CreateInstance)
                    .Cast<IInstaller>()
                    .OrderBy(x => x.Order)
                    .ToList();

                Log.Information(StringMessages.FoundInstallersToRegister, installers.Count);

                foreach (var installer in installers)
                {
                    var installerName = installer.GetType().Name;
                    Log.Debug(StringMessages.InstallingServicesFrom, installerName, installer.Order);
                    
                    installer.InstallServices(services, configuration);
                }

                Log.Information(StringMessages.AllServicesInstalledSuccessfully);
                return services;
            }
            catch (Exception ex)
            {
                Log.Error(ex, StringMessages.FailedToInstallServicesFromAssembly);
                throw;
            }
        }
    }
}
