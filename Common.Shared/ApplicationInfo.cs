using System.Reflection;

namespace Common.Shared
{
    /// <summary>
    /// Application information and version details
    /// </summary>
    public static class ApplicationInfo
    {
        /// <summary>
        /// Application name
        /// </summary>
        public static string Name => "SampleProject.API";

        /// <summary>
        /// Application version
        /// </summary>
        public static string Version => "0.0.0";

        /// <summary>
        /// Application description
        /// </summary>
        public static string Description => "A comprehensive .NET 9 example showcasing modern development practices";

        /// <summary>
        /// Application owner/company
        /// </summary>
        public static string Owner => "FlowCore";

        /// <summary>
        /// Application copyright
        /// </summary>
        public static string Copyright => $"© {DateTime.UtcNow.Year} {Owner}. All rights reserved.";

        /// <summary>
        /// Application release date
        /// </summary>
        public static DateTime ReleaseDate => new DateTime(2025, 10, 27);

        /// <summary>
        /// Application build date
        /// </summary>
        public static DateTime BuildDate => DateTime.UtcNow;

        /// <summary>
        /// Runtime version
        /// </summary>
        public static string RuntimeVersion => Environment.Version.ToString();

        /// <summary>
        /// Target framework
        /// </summary>
        public static string TargetFramework => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown";

        /// <summary>
        /// Gets formatted application information
        /// </summary>
        /// <returns>Formatted application information string</returns>
        public static string GetFormattedInfo()
        {
            return $"{Name} v{Version} by {Owner}";
        }

        /// <summary>
        /// Gets detailed application information
        /// </summary>
        /// <returns>Detailed application information</returns>
        public static object GetDetailedInfo()
        {
            return new
            {
                Name,
                Version,
                Description,
                Owner,
                Copyright,
                ReleaseDate = ReleaseDate.ToString("yyyy-MM-dd"),
                BuildDate = BuildDate.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                RuntimeVersion,
                TargetFramework
            };
        }
    }
}
