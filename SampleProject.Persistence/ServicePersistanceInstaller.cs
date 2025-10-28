using SampleProject.Persistence.Data;
using SampleProject.Persistence.Repositories;

namespace SampleProject.Persistence
{
    /// <summary>
    /// Service installer for persistence layer dependencies
    /// </summary>
    public static class ServicePersistanceInstaller
    {
        /// <summary>
        /// Installs persistence services into the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Updated service collection</returns>
        public static IServiceCollection InstallPersistanceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get database options
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>()
                ?? throw new InvalidOperationException(StringMessages.DatabaseConfigurationNotFound);

            // Configure database context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (databaseOptions.UseInMemory)
                {
                    options.UseInMemoryDatabase(databaseOptions.DatabaseName ?? StringMessages.DefaultDatabaseName);
                }
                else
                {
                    options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(30);
                    });
                }

                // Enable sensitive data logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == StringMessages.DevelopmentEnvironment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Register repositories using new interfaces from Application layer
            services.AddScoped(typeof(SampleProject.Application.Interfaces.Persistence.IRepository<>), typeof(Repository<>));

            // Register Unit of Work using new interface from Application layer
            services.AddScoped<SampleProject.Application.Interfaces.Persistence.IUnitOfWork, UnitOfWork.UnitOfWork>();

            return services;
        }
    }
}