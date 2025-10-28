using SampleProject.Domain.Common;
using SampleProject.Domain.Responses;

namespace SampleProject.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    /// Command to logout user
    /// </summary>
    public class LogoutCommand : IRequest<Result<LogoutResponse>>
    {
        // No additional properties needed for logout
    }

    /// <summary>
    /// Response model for logout
    /// </summary>
    public class LogoutResponse
    {
        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
