namespace Common.Options
{
    /// <summary>
    /// Serilog configuration options
    /// </summary>
    public class SerilogOptions
    {
        /// <summary>
        /// Enable console logging
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Enable file logging
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;

        /// <summary>
        /// Maximum file size in MB
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 10;

        /// <summary>
        /// Maximum number of files to keep
        /// </summary>
        public int MaxFilesToKeep { get; set; } = 30;

        /// <summary>
        /// Minimum log level
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath { get; set; } = "Logs/application-.log";

        /// <summary>
        /// Detailed log file path
        /// </summary>
        public string DetailedLogFilePath { get; set; } = "Logs/detailed-.log";
    }
}
