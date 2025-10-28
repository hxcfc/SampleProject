using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SampleProject.Infrastructure.Interfaces;
using System.Diagnostics;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Service for system monitoring and metrics collection
    /// </summary>
    public class SystemMonitoringService : ISystemMonitoringService, IHostedService
    {
        private readonly Process _currentProcess;
        private readonly ILogger<SystemMonitoringService> _logger;
        private readonly IAdvancedMetricsService _metricsService;
        private readonly Timer? _monitoringTimer;
        private readonly MonitoringOptions _options;

        /// <summary>
        /// Initializes a new instance of the SystemMonitoringService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="metricsService">Advanced metrics service</param>
        /// <param name="options">Monitoring options</param>
        public SystemMonitoringService(ILogger<SystemMonitoringService> logger, IAdvancedMetricsService metricsService, IOptions<MonitoringOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _currentProcess = Process.GetCurrentProcess();

            if (_options.Enabled)
            {
                _monitoringTimer = new Timer(CollectSystemMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(_options.CollectionIntervalSeconds));
                _logger.LogInformation(StringMessages.SystemMonitoringServiceInitialized, _options.CollectionIntervalSeconds);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _monitoringTimer?.Dispose();
            _currentProcess?.Dispose();
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StringMessages.SystemMonitoringServiceStarted);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _monitoringTimer?.Dispose();
            _logger.LogInformation(StringMessages.SystemMonitoringServiceStopped);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Collects CPU usage metrics
        /// </summary>
        private void CollectCpuMetrics()
        {
            try
            {
                // Get process CPU usage
                var processCpu = _currentProcess.TotalProcessorTime.TotalMilliseconds;
                _metricsService.RecordCustomMetric(StringMessages.ProcessCpuTimeMetric, processCpu, new Dictionary<string, string>
                {
                    { "type", StringMessages.ProcessTypeTag }
                });

                // Get GC collections
                var gcCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
                _metricsService.RecordCustomMetric(StringMessages.GcCollectionsMetric, gcCollections, new Dictionary<string, string>
                {
                    { "type", StringMessages.TotalTypeTag }
                });

                _logger.LogDebug(StringMessages.CpuMetricsCollected, processCpu, gcCollections);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, StringMessages.FailedToCollectCpuMetrics);
            }
        }

        /// <summary>
        /// Collects memory usage metrics
        /// </summary>
        private void CollectMemoryMetrics()
        {
            try
            {
                // Get process memory usage
                var processMemory = _currentProcess.WorkingSet64;
                _metricsService.RecordMemoryUsage(processMemory);

                // Get system memory info (if available)
                var systemMemory = GC.GetTotalMemory(false);
                _metricsService.RecordCustomMetric(StringMessages.SystemMemoryTotalMetric, systemMemory, new Dictionary<string, string>
                {
                    { "type", StringMessages.GcTotalTypeTag }
                });

                _logger.LogDebug(StringMessages.MemoryMetricsCollected, processMemory, systemMemory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, StringMessages.FailedToCollectMemoryMetrics);
            }
        }

        /// <summary>
        /// Collects system metrics
        /// </summary>
        /// <param name="state">Timer state</param>
        private void CollectSystemMetrics(object? state)
        {
            try
            {
                if (_options.EnableMemoryMonitoring)
                {
                    CollectMemoryMetrics();
                }

                if (_options.EnableCpuMonitoring)
                {
                    CollectCpuMetrics();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, StringMessages.ErrorCollectingSystemMetrics);
            }
        }
    }
}