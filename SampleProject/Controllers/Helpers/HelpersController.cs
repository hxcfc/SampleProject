using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Common.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Helpers
{
    /// <summary>
    /// Helper API utilities and helper endpoints
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/helpers")]
    [SwaggerTag("Utility endpoints for server information and configuration")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class HelpersController : Controller
    {
        /// <summary>
        /// Gets server time
        /// </summary>
        /// <returns>Current server time in UTC</returns>
        [SwaggerOperation(
            Summary = "Get server time",
            Description = "Returns current server time in UTC and local timezone, including Unix timestamp. Useful for synchronizing client applications or debugging time-related issues.",
            OperationId = "GetServerTime"
        )]
        [SwaggerResponse(200, "Server time information returned successfully")]
        [HttpGet("time")]
        public IActionResult GetServerTime()
        {
            return Ok(new
            {
                utc = DateTime.UtcNow,
                local = DateTime.Now,
                timezone = TimeZoneInfo.Local.Id,
                unix = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()
            });
        }

        /// <summary>
        /// Gets application configuration summary (non-sensitive)
        /// </summary>
        /// <returns>Configuration summary</returns>
        [SwaggerOperation(
            Summary = "Get application configuration summary",
            Description = "Returns non-sensitive application configuration information including environment, version, framework, runtime details, and build information. This endpoint does not expose sensitive configuration data like connection strings or API keys.",
            OperationId = "GetConfigSummary"
        )]
        [SwaggerResponse(200, "Configuration summary returned successfully")]
        [HttpGet("config")]
        public IActionResult GetConfigSummary()
        {
            return Ok(new
            {
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                version = ApplicationInfo.Version,
                framework = ApplicationInfo.TargetFramework,
                runtime = ApplicationInfo.RuntimeVersion,
                buildDate = ApplicationInfo.BuildDate,
                releaseDate = ApplicationInfo.ReleaseDate
            });
        }

        /// <summary>
        /// Gets available API versions
        /// </summary>
        /// <returns>Available API versions</returns>
        [SwaggerOperation(
            Summary = "Get available API versions",
            Description = "Returns information about available API versions including the current version, all supported versions, and deprecated versions. This helps clients determine which API version to use and when to migrate to newer versions.",
            OperationId = "GetApiVersions"
        )]
        [SwaggerResponse(200, "API version information returned successfully")]
        [HttpGet("versions")]
        public IActionResult GetApiVersions()
        {
            return Ok(new
            {
                current = "1.0",
                supported = new[] { "1.0" },
                deprecated = new string[0]
            });
        }
    }
}
