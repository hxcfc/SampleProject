namespace SampleProject.Installers.Extensions
{
    /// <summary>
    /// Extension methods for WebApplication to configure services
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Configures all services in the assembly in the correct order
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application</returns>
        public static WebApplication ConfigureServicesInAssembly(this WebApplication app)
        {
            try
            {
                var installers = typeof(Program).Assembly.ExportedTypes
                    .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(Activator.CreateInstance)
                    .Cast<IInstaller>()
                    .OrderBy(x => x.Order)
                    .ToList();

                Log.Information(StringMessages.FoundInstallersToConfigure, installers.Count);

                foreach (var installer in installers)
                {
                    var installerName = installer.GetType().Name;
                    Log.Debug(StringMessages.ConfiguringServicesFrom, installerName, installer.Order);
                    
                    installer.ConfigureServices(app);
                }

                Log.Information(StringMessages.AllServicesConfiguredSuccessfully);
                return app;
            }
            catch (Exception ex)
            {
                Log.Error(ex, StringMessages.FailedToConfigureServicesFromAssembly);
                throw;
            }
        }
    }
}
