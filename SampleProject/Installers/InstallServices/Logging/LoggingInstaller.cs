namespace SampleProject.Installers.InstallServices.Logging
{
    /// <summary>
    /// Installer for logging services
    /// </summary>
    public class LoggingInstaller : IInstaller
    {
        /// <summary>
        /// Gets the installation order priority
        /// </summary>
        public int Order => 6;

        /// <summary>
        /// Configures Serilog logging
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public static void ConfigureSerilog(IConfiguration configuration)
        {
            var loggerConfig = CreateLoggerConfiguration(configuration, Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");
            Log.Logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Configures Serilog with WebApplicationBuilder
        /// </summary>
        /// <param name="builder">Web application builder</param>
        /// <param name="configuration">Configuration</param>
        public static void ConfigureSerilog(WebApplicationBuilder builder, IConfiguration configuration)
        {
            var loggerConfig = CreateLoggerConfiguration(configuration, builder.Environment.EnvironmentName);
            builder.Host.UseSerilog(loggerConfig.CreateLogger());
        }

        /// <summary>
        /// Configures services in the web application
        /// </summary>
        /// <param name="app">Web application</param>
        public void ConfigureServices(WebApplication app)
        {
            // Log application startup
            Log.Information(StringMessages.ApplicationStartupMessage());
            Log.Information(StringMessages.ApplicationRunningInEnvironment, app.Environment.EnvironmentName);
        }

        /// <summary>
        /// Installs logging services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Serilog
            ConfigureSerilog(configuration);

            // Add Serilog to the service collection
            services.AddSerilog();
        }

        /// <summary>
        /// Configures file logging options
        /// </summary>
        /// <param name="loggerConfig">Logger configuration</param>
        /// <param name="loggingOptions">Logging options</param>
        private static void ConfigureFileLogging(LoggerConfiguration loggerConfig, SerilogOptions loggingOptions)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "application-.log");

            loggerConfig.WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 5,
                fileSizeLimitBytes: 5 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8); // UTF-8 encoding for emoji support

            // Detailed logging - tylko w Development
            // Using text format with structured properties instead of JSON to avoid escaped quotes
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var detailedLogPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "detailed-.log");
                loggerConfig.WriteTo.File(
                    path: detailedLogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 3,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                    encoding: System.Text.Encoding.UTF8); // UTF-8 encoding for emoji and special characters
            }
        }

        /// <summary>
        /// Creates a configured LoggerConfiguration
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="environmentName">Environment name</param>
        /// <returns>Configured LoggerConfiguration</returns>
        private static LoggerConfiguration CreateLoggerConfiguration(IConfiguration configuration, string environmentName)
        {
            var loggingOptions = configuration.GetSection("Serilog").Get<SerilogOptions>() ?? new SerilogOptions();

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", ApplicationInfo.Name)
                .Enrich.WithProperty("Version", ApplicationInfo.Version)
                .Enrich.WithProperty("Environment", environmentName);

            // Configure console logging
            if (loggingOptions.EnableConsoleLogging)
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Configure file logging
            if (loggingOptions.EnableFileLogging)
            {
                ConfigureFileLogging(loggerConfig, loggingOptions);
            }

            return loggerConfig;
        }
    }
}