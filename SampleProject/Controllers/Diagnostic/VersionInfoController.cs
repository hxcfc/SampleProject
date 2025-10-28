using Common.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Swashbuckle.AspNetCore.Annotations;

namespace SampleProject.Controllers.Diagnostic
{
    /// <summary>
    /// Version information controller
    /// Provides detailed application version and build information for diagnostics
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/version")]
    [SwaggerTag("Application version and build information")]
    [ApiExplorerSettings(GroupName = "v1-Diagnostic")]
    public class VersionInfoController : Controller
    {
        /// <summary>
        /// Gets detailed application version information
        /// </summary>
        /// <returns>Detailed application version and build information</returns>
        /// <response code="200">Returns detailed version information</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get detailed application version information",
            Description = "Returns comprehensive application version and build information including version number, build date, runtime details, and more.",
            OperationId = "GetVersionInfo"
        )]
        [SwaggerResponse(200, "Detailed version information", typeof(object))]
        public IActionResult GetInfo()
        {
            return Ok(ApplicationInfo.GetDetailedInfo());
        }
    }
}
