using Common.Options;
using Microsoft.EntityFrameworkCore;
using SampleProject.Persistence.Data;
using SampleProject.Domain.Entities;
using SampleProject.Application.Interfaces;
using SampleProject.Application.Interfaces.SampleProject.Authorization;
using SampleProject.Application.Implementations;
using SampleProject.Infrastructure.Implementations;

namespace SampleProject.Installers.InstallServices.Database
{
    /// <summary>
    /// Installer for database services
    /// </summary>
    public class DatabaseInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 2;

        /// <summary>
        /// Configures database services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // Ensure database is created
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                context.Database.EnsureCreated();
                Log.Information(StringMessages.DatabaseEnsuredCreatedSuccessfully);
            }
            catch (Exception ex)
            {
                Log.Error(ex, StringMessages.FailedToEnsureDatabaseCreated);
                throw;
            }
        }

        /// <summary>
        /// Installs database services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

            // Configure Entity Framework
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (databaseOptions.UseInMemory)
                {
                    options.UseInMemoryDatabase(databaseOptions.DatabaseName ?? StringMessages.DefaultDatabaseName);
                    Log.Information(StringMessages.UsingInMemoryDatabase, databaseOptions.DatabaseName);
                }
                else
                {
                    options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
                        npgsqlOptions.EnableRetryOnFailure(3);
                    });
                    Log.Information(StringMessages.UsingPostgreSQLDatabase,
                        databaseOptions.ConnectionString.Replace(databaseOptions.ConnectionString.Split(';')[0], StringMessages.HostMasked));
                }

                // Configure sensitive data logging
                if (databaseOptions.EnableSensitiveDataLogging)
                {
                    options.EnableSensitiveDataLogging();
                    Log.Warning(StringMessages.SensitiveDataLoggingEnabled);
                }

                // Configure detailed errors
                options.EnableDetailedErrors();
            });

            // Note: IRepository and IUnitOfWork are registered in ServicePersistanceInstaller
            // to avoid duplication and maintain proper separation of concerns

            // Register services needed by MediatR handlers
            services.AddScoped<IPasswordService, SampleProject.Application.Implementations.PasswordService>();
            services.AddTransient<IAuthorization, Authorization>();
            // Note: IJwtService is now registered in InfrastructureInstaller

            // Register DbContext factory for background services (as scoped to avoid singleton issues)
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                if (databaseOptions.UseInMemory)
                {
                    options.UseInMemoryDatabase(databaseOptions.DatabaseName ?? StringMessages.DefaultDatabaseName);
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

            Log.Information(StringMessages.DatabaseServicesConfiguredSuccessfully);
        }
    }
}