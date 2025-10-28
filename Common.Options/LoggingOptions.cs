namespace Common.Options
{
    /// <summary>
    /// Logging configuration options
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Minimum log level
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath { get; set; } = "logs/app.log";

        /// <summary>
        /// Maximum log file size in MB
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 10;

        /// <summary>
        /// Number of log files to keep
        /// </summary>
        public int MaxFilesToKeep { get; set; } = 5;

        /// <summary>
        /// Enable console logging
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Enable file logging
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;
    }
}
