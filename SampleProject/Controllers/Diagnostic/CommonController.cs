using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Common.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Diagnostic
{
    /// <summary>
    /// Common API utilities and helper endpoints
    /// Provides utility functions like server time, configuration summary, and API version information
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/common")]
    [SwaggerTag("Common utility endpoints for API information and utilities")]
    [ApiExplorerSettings(GroupName = "v1-Diagnostic")]
    public class CommonController : Controller
    {
        /// <summary>
        /// Gets server time
        /// </summary>
        /// <returns>Current server time in UTC</returns>
        /// <response code="200">Returns server time information</response>
        [HttpGet("time")]
        [SwaggerOperation(
            Summary = "Get server time",
            Description = "Returns current server time in UTC and local timezone, including Unix timestamp.",
            OperationId = "GetCommonServerTime"
        )]
        [SwaggerResponse(200, "Server time information", typeof(object))]
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
        /// <response code="200">Returns configuration summary</response>
        [HttpGet("config")]
        [SwaggerOperation(
            Summary = "Get application configuration summary",
            Description = "Returns non-sensitive application configuration information including environment, version, framework, and build details.",
            OperationId = "GetCommonConfigSummary"
        )]
        [SwaggerResponse(200, "Configuration summary", typeof(object))]
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
        /// <response code="200">Returns available API versions</response>
        [HttpGet("versions")]
        [SwaggerOperation(
            Summary = "Get available API versions",
            Description = "Returns information about available API versions including current, supported, and deprecated versions.",
            OperationId = "GetCommonApiVersions"
        )]
        [SwaggerResponse(200, "Available API versions", typeof(object))]
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
