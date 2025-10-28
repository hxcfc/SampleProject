using Microsoft.Extensions.Logging;

namespace Common.Shared
{
    /// <summary>
    /// Handles application startup display and branding
    /// </summary>
    public static class StartupDisplay
    {
        /// <summary>
        /// Displays the FlowCore ASCII art logo
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public static void DisplayLogo(ILogger logger)
        {
            logger.LogInformation("-------------------------------------------------------------------------------------------------------");
            logger.LogInformation(string.Empty);
            logger.LogInformation("  ███████╗██╗      ██████╗ ██╗    ██╗ ██████╗ ██████╗ ██████╗ ███████╗");
            logger.LogInformation("  ██╔════╝██║     ██╔═══██╗██║    ██║██╔════╝██╔═══██╗██╔══██╗██╔════╝");
            logger.LogInformation("  █████╗  ██║     ██║   ██║██║ █╗ ██║██║     ██║   ██║██████╔╝█████╗  ");
            logger.LogInformation("  ██╔══╝  ██║     ██║   ██║██║███╗██║██║     ██║   ██║██╔══██╗██╔══╝  ");
            logger.LogInformation("  ██║     ███████╗╚██████╔╝╚███╔███╔╝╚██████╗╚██████╔╝██║  ██║███████╗");
            logger.LogInformation("  ╚═╝     ╚══════╝ ╚═════╝  ╚══╝╚══╝  ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝");
            logger.LogInformation(string.Empty);
        }

        /// <summary>
        /// Displays application information
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public static void DisplayApplicationInfo(ILogger logger)
        {
            logger.LogInformation("┌─────────────────────────────────────────────────────────────────────────────────────────────┐");
            logger.LogInformation("│                                    APPLICATION INFO                                        │");
            logger.LogInformation("├─────────────────────────────────────────────────────────────────────────────────────────────┤");
            logger.LogInformation("│  Application: {ApplicationName,-18} │  Version: {Version,-8} │  Owner: {Owner,-13} │", 
                ApplicationInfo.Name, ApplicationInfo.Version, ApplicationInfo.Owner);
            logger.LogInformation("│  Release Date: {ReleaseDate,-13} │  Build Date: {BuildDate,-13} │  Runtime: {Runtime,-8} │", 
                ApplicationInfo.ReleaseDate.ToString("yyyy-MM-dd"), 
                ApplicationInfo.BuildDate.ToString("yyyy-MM-dd"), 
                ApplicationInfo.RuntimeVersion);
            logger.LogInformation("│  Framework: {Framework,-18} │  Environment: {Environment,-13} │  Port: {Port,-8} │", 
                ApplicationInfo.TargetFramework, 
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                "15553");
            logger.LogInformation("└─────────────────────────────────────────────────────────────────────────────────────────────┘");
            logger.LogInformation(string.Empty);
        }

        /// <summary>
        /// Displays startup banner with all information
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public static void DisplayStartupBanner(ILogger logger)
        {
            DisplayLogo(logger);
            DisplayApplicationInfo(logger);
            logger.LogInformation("Application started successfully!");
            logger.LogInformation("Health checks available at: /health");
            logger.LogInformation("API documentation available at: /swagger");
            logger.LogInformation("-------------------------------------------------------------------------------------------------------");
        }

        /// <summary>
        /// Displays environment-specific startup messages
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="environment">Environment name</param>
        public static void DisplayEnvironmentInfo(ILogger logger, string environment)
        {
            switch (environment.ToLowerInvariant())
            {
                case "development":
                    logger.LogInformation("🔧 Development Mode - Enhanced logging enabled");
                    logger.LogInformation("🐛 Debug information available");
                    break;
                case "staging":
                    logger.LogInformation("🧪 Staging Mode - Production-like environment");
                    break;
                case "production":
                    logger.LogInformation("🏭 Production Mode - Optimized for performance");
                    break;
                default:
                    logger.LogInformation("❓ Unknown Environment: {Environment}", environment);
                    break;
            }
        }

        /// <summary>
        /// Logs detailed application information
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="environment">Environment name</param>
        public static void LogApplicationDetails(ILogger logger, string environment)
        {
            logger.LogInformation("Environment: {Environment}", environment);
            logger.LogInformation("Application: {Application} v{Version}", ApplicationInfo.Name, ApplicationInfo.Version);
            logger.LogInformation("Release Date: {ReleaseDate}", ApplicationInfo.ReleaseDate);
            logger.LogInformation("Runtime: {Runtime}", ApplicationInfo.RuntimeVersion);
            logger.LogInformation("Framework: {Framework}", ApplicationInfo.TargetFramework);
            logger.LogInformation("Description: {Description}", ApplicationInfo.Description);
            logger.LogInformation("Copyright: {Copyright}", ApplicationInfo.Copyright);
        }
    }
}
