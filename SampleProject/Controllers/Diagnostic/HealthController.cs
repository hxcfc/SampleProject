using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Common.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Diagnostic
{
    /// <summary>
    /// Health and utility endpoints controller
    /// Provides health checks, ping responses, and application information for monitoring and diagnostics
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/health")]
    [SwaggerTag("Health and monitoring endpoints for application diagnostics")]
    [ApiExplorerSettings(GroupName = "v1-Diagnostic")]
    public class HealthController : Controller
    {
        /// <summary>
        /// Gets application health status
        /// </summary>
        /// <returns>Health status information</returns>
        /// <response code="200">Returns health status information</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get application health status",
            Description = "Returns the current health status of the application including status, timestamp, version, and environment information.",
            OperationId = "GetHealth"
        )]
        [SwaggerResponse(200, "Health status information", typeof(object))]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = ApplicationInfo.Version,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        /// <summary>
        /// Gets application ping response
        /// </summary>
        /// <returns>Simple ping response</returns>
        /// <response code="200">Returns ping response</response>
        [HttpGet("ping")]
        [SwaggerOperation(
            Summary = "Ping the application",
            Description = "Simple ping endpoint to check if the application is responding. Returns a pong message with timestamp.",
            OperationId = "Ping"
        )]
        [SwaggerResponse(200, "Ping response", typeof(object))]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Gets application information
        /// </summary>
        /// <returns>Application information</returns>
        /// <response code="200">Returns detailed application information</response>
        [HttpGet("info")]
        [SwaggerOperation(
            Summary = "Get detailed application information",
            Description = "Returns comprehensive application information including version, build date, runtime details, and more.",
            OperationId = "GetApplicationInfo"
        )]
        [SwaggerResponse(200, "Detailed application information", typeof(object))]
        public IActionResult GetInfo()
        {
            return Ok(ApplicationInfo.GetDetailedInfo());
        }
    }
}
