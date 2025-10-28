using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace SampleProject.Application
{
    /// <summary>
    /// Installer for application services
    /// </summary>
    public static class ServiceApplicationInstaller
    {
        /// <summary>
        /// Installs application services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection InstallApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Install application services
            // Note: MediatR is registered in MediatRInstaller to avoid duplicate registrations
            
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            return services;
        }
    }
}