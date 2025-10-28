using MediatR;
using SampleProject.Domain.Common;

namespace SampleProject.Application.Features.Users.Queries.CheckEmailAvailability
{
    /// <summary>
    /// Query to check if email is available for registration
    /// </summary>
    public class CheckEmailAvailabilityQuery : IRequest<Result<bool>>
    {
        /// <summary>
        /// Email address to check
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of CheckEmailAvailabilityQuery
        /// </summary>
        /// <param name="email">Email address to check</param>
        public CheckEmailAvailabilityQuery(string email)
        {
            Email = email;
        }
    }
}
