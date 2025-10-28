using Common.Shared.Interfaces;
using Microsoft.Extensions.Hosting;

namespace SampleProject.Infrastructure.Services
{
    /// <summary>
    /// Hosted service for seeding database on application startup
    /// </summary>
    public class DatabaseSeedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseSeedHostedService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeedHostedService"/> class
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="logger">Logger instance</param>
        public DatabaseSeedHostedService(IServiceProvider serviceProvider, ILogger<DatabaseSeedHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting database seeding hosted service");

                using var scope = _serviceProvider.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
                
                await seeder.SeedAsync();
                
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed database during startup");
                // Don't throw - we don't want to crash the application
            }
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Database seeding hosted service stopped");
            return Task.CompletedTask;
        }
    }
}
