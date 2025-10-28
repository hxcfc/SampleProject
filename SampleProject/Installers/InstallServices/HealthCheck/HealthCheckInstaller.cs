using SampleProject.Persistence.Data;

namespace SampleProject.Installers.InstallServices.HealthCheck
{
    /// <summary>
    /// Installer for health check services
    /// </summary>
    public class HealthCheckInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 8;

        /// <summary>
        /// Configures health check services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            var healthCheckOptions = app.Configuration.GetSection("HealthCheck").Get<Common.Options.HealthCheckOptions>() ?? new Common.Options.HealthCheckOptions();

            if (!healthCheckOptions.Enabled)
            {
                return;
            }

            // Configure health check endpoints with security
            var healthCheckBuilder = app.MapHealthChecks($"/{healthCheckOptions.EndpointPath.TrimStart('/')}", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = healthCheckOptions.EnableDetailedResponses ? WriteHealthCheckResponse : WriteSimpleHealthCheckResponse,
                Predicate = check => check.Tags.Contains("ready")
            });

            var liveHealthCheckBuilder = app.MapHealthChecks($"/{healthCheckOptions.EndpointPath.TrimStart('/')}/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = healthCheckOptions.EnableDetailedResponses ? WriteHealthCheckResponse : WriteSimpleHealthCheckResponse,
                Predicate = check => check.Tags.Contains("live")
            });

            // SECURITY: In production, require authentication for health checks
            if (!app.Environment.IsDevelopment())
            {
                healthCheckBuilder.RequireAuthorization();
                liveHealthCheckBuilder.RequireAuthorization();
                Log.Warning("Health check endpoints secured with authentication in Production environment");
            }

            // Configure health check UI if enabled and in development
            if (healthCheckOptions.EnableUI && app.Environment.IsDevelopment())
            {
                app.MapHealthChecksUI(options =>
                {
                    options.UIPath = $"/{healthCheckOptions.UIEndpointPath.TrimStart('/')}";
                });

                Log.Information("Health check UI enabled at /{HealthCheckUIEndpoint}", healthCheckOptions.UIEndpointPath);
            }

            Log.Information(StringMessages.HealthCheckEndpointsConfiguredAt, healthCheckOptions.EndpointPath);
        }

        /// <summary>
        /// Installs health check services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var healthCheckOptions = configuration.GetSection("HealthCheck").Get<Common.Options.HealthCheckOptions>() ?? new Common.Options.HealthCheckOptions();

            if (!healthCheckOptions.Enabled)
            {
                Log.Information(StringMessages.HealthChecksDisabledInConfiguration);
                return;
            }

            var healthChecksBuilder = services.AddHealthChecks();

            // Add database health check
            if (healthCheckOptions.EnableDatabaseHealthCheck)
            {
                healthChecksBuilder.AddDbContextCheck<ApplicationDbContext>(
                    name: StringMessages.DatabaseHealthCheckName,
                    tags: StringMessages.DatabaseHealthCheckTags.Split(','));
            }

            // Add memory health check
            if (healthCheckOptions.EnableMemoryHealthCheck)
            {
                healthChecksBuilder.AddCheck(
                    name: StringMessages.MemoryHealthCheckName,
                    check: () => HealthCheckResult.Healthy(StringMessages.MemoryHealthCheckDescription),
                    tags: new[] { StringMessages.MemoryHealthCheckTags });
            }

            // Add basic health check
            healthChecksBuilder.AddCheck(
                name: StringMessages.BasicHealthCheckName,
                check: () => HealthCheckResult.Healthy(StringMessages.BasicHealthCheckDescription),
                tags: StringMessages.BasicHealthCheckTags.Split(','));

            // Configure health check UI if enabled
            if (healthCheckOptions.EnableUI)
            {
                services.AddHealthChecksUI(options =>
                {
                    options.SetEvaluationTimeInSeconds(15);
                    options.MaximumHistoryEntriesPerEndpoint(60);
                    options.SetApiMaxActiveRequests(1);
                    options.AddHealthCheckEndpoint(StringMessages.HealthCheckUIEndpointName, StringMessages.HealthCheckUIEndpointUrl.Replace("{HealthCheckEndpoint}", healthCheckOptions.EndpointPath.TrimStart('/')));
                }).AddInMemoryStorage();
            }

            Log.Information(StringMessages.HealthCheckServicesConfiguredSuccessfully);
        }

        /// <summary>
        /// Writes detailed health check response
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="result">Health check result</param>
        /// <returns>Task</returns>
        private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = StringMessages.HealthCheckResponseContentType;

            var response = new
            {
                status = result.Status.ToString(),
                totalDuration = result.TotalDuration.ToString(StringMessages.HealthCheckDurationFormat),
                checks = result.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.ToString(StringMessages.HealthCheckDurationFormat),
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    exception = entry.Value.Exception?.Message
                })
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Writes simple health check response
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="result">Health check result</param>
        /// <returns>Task</returns>
        private static async Task WriteSimpleHealthCheckResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = StringMessages.HealthCheckResponseContentType;

            var response = new
            {
                status = result.Status.ToString(),
                totalDuration = result.TotalDuration.ToString(StringMessages.HealthCheckDurationFormat)
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}