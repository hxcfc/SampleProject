using Common.Options;
using Microsoft.EntityFrameworkCore;
using SampleProject.Persistence.Data;
using SampleProject.Persistence.Repositories;
using SampleProject.Persistence.UnitOfWork;
using SampleProject.Application.Interfaces.Persistence;

namespace SampleProject.Installers.InstallServices.Data
{
    /// <summary>
    /// Installer for data services (Database, Unit of Work, Repositories)
    /// Single responsibility: All data access concerns in one place
    /// </summary>
    public class DataInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 3; // After observability, before security

        /// <summary>
        /// Installs data services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Database Context
            ConfigureDatabaseContext(services, configuration);

            // Configure Unit of Work
            ConfigureUnitOfWork(services, configuration);

            // Configure Repositories
            ConfigureRepositories(services, configuration);

            // Configure Database Seeding
            ConfigureDatabaseSeeding(services, configuration);

            Log.Information("🗄️ Data services configured successfully");
        }

        /// <summary>
        /// Configures database context
        /// </summary>
        private static void ConfigureDatabaseContext(IServiceCollection services, IConfiguration configuration)
        {
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (databaseOptions.UseInMemory)
                {
                    options.UseInMemoryDatabase(databaseOptions.DatabaseName ?? "SampleProjectDb");
                    Log.Information("Using In-Memory database: {DatabaseName}", databaseOptions.DatabaseName ?? "SampleProjectDb");
                }
                else
                {
                    if (string.IsNullOrEmpty(databaseOptions.ConnectionString))
                    {
                        throw new InvalidOperationException("Database connection string is required when not using in-memory database");
                    }

                    options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });

                    Log.Information("Using PostgreSQL database with connection string: {ConnectionString}", 
                        MaskConnectionString(databaseOptions.ConnectionString));
                }

                // Configure additional options
                ConfigureDbContextOptions(options, databaseOptions);
            });

            // Register DbContext factory for background services
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                if (databaseOptions.UseInMemory)
                {
                    options.UseInMemoryDatabase(databaseOptions.DatabaseName ?? "SampleProjectDb");
                }
                else
                {
                    options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
                        npgsqlOptions.EnableRetryOnFailure(3);
                    });
                }
            }, ServiceLifetime.Scoped);

            Log.Information("Database context configured successfully");
        }

        /// <summary>
        /// Configures additional DbContext options
        /// </summary>
        private static void ConfigureDbContextOptions(DbContextOptionsBuilder options, DatabaseOptions databaseOptions)
        {
            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Configure query tracking behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);

            // Configure connection resiliency
            if (!databaseOptions.UseInMemory)
            {
                options.EnableServiceProviderCaching();
                options.EnableSensitiveDataLogging(false); // Disable in production
            }
        }

        /// <summary>
        /// Configures Unit of Work
        /// </summary>
        private static void ConfigureUnitOfWork(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            Log.Information("Unit of Work configured successfully");
        }

        /// <summary>
        /// Configures repositories
        /// </summary>
        private static void ConfigureRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Register generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // TODO: Register specific repositories when interfaces are available

            Log.Information("Repositories configured successfully");
        }

        /// <summary>
        /// Configures database seeding
        /// </summary>
        private static void ConfigureDatabaseSeeding(IServiceCollection services, IConfiguration configuration)
        {
            var seedingOptions = configuration.GetSection("DatabaseSeeding").Get<DatabaseSeedingOptions>() ?? new DatabaseSeedingOptions();

            if (!seedingOptions.Enabled)
            {
                Log.Information("Database seeding disabled in configuration");
                return;
            }

            // TODO: Register database seeder when interface is available

            // Register hosted service for database seeding
            services.AddHostedService<SampleProject.Infrastructure.Services.DatabaseSeedHostedService>();

            Log.Information("Database seeding configured successfully");
        }

        /// <summary>
        /// Configures data services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // Database seeding is handled by DatabaseSeedHostedService
            // No additional configuration needed here

            Log.Information("🗄️ Data services middleware configured successfully");
        }

        /// <summary>
        /// Masks sensitive information in connection string for logging
        /// </summary>
        private static string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return "Not configured";
            }

            // Simple masking - replace password with ***
            var masked = connectionString;
            var passwordMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Password=([^;]+)");
            if (passwordMatch.Success)
            {
                masked = masked.Replace(passwordMatch.Groups[1].Value, "***");
            }

            return masked;
        }
    }
}
