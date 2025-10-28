using Common.Shared.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Interfaces;
using SampleProject.Infrastructure.Implementations;
using SampleProject.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace SampleProject.Infrastructure
{
    /// <summary>
    /// Installer for infrastructure services
    /// </summary>
    public static class InfrastructureInstaller
    {
        /// <summary>
        /// Installs infrastructure services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection InstallInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
        // Register HTTP context accessor for accessing HttpContext in services
        services.AddHttpContextAccessor();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();
            
            // Register caching services
            services.AddMemoryCache();
            services.AddDistributedMemoryCache(); // In-memory distributed cache for development
            services.AddScoped<ICacheService, CacheService>();
            
            // Register security headers service
            services.AddSingleton<ISecurityHeadersService, SecurityHeadersService>();
            
            // Register JWT service
            services.AddScoped<IJwtService, Implementations.JwtService>();
            services.AddScoped<IExtendedJwtService, Implementations.JwtService>();
            
            // Note: IAuthorization and IPasswordService are now registered in DatabaseInstaller
            // to ensure they are available before MediatR registration
            
            // Register database seeder
            services.AddScoped<IDatabaseSeeder, Implementations.DatabaseSeeder>();
            
            // Register hosted service for database seeding
            services.AddHostedService<Services.DatabaseSeedHostedService>();
            
            // Register user services
            services.AddScoped<IUserService, Implementations.UserService>();
            services.AddScoped<IUserRepository, Implementations.UserRepository>();
            
            return services;
        }
    }
}
