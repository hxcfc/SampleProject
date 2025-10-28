using Microsoft.Extensions.Hosting;

namespace SampleProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for system monitoring service
    /// </summary>
    public interface ISystemMonitoringService : IHostedService, IDisposable
    {
    }
}